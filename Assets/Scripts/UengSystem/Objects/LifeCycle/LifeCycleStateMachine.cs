using System;
using UengSystem.UDebug;

namespace UengSystem.Objects.LifeCycle {
	public sealed class LifeCycleStateMachine {

		// 인스턴스 프로퍼티
		public UObject target { get; }
		
		public LifeCyclePhase phase = LifeCyclePhase.Released;
		public LifeCycleState currentState;
		
		public long lifeNumber;
		public long executionNumber;
		
		public bool isPrepared;
		public bool isFaulted;
		public bool isShuttingDown;
		
		private Getting GettingState;
		private Releasing ReleasingState;
		
		private bool Completing;
		private bool Starting;
		private bool Entering;
		
		private bool InstantReleaseRequested;
		private bool ReleaseStarted;
		
		private bool PendingCompletion;
		private bool PendingRelease;
		private bool PendingReleaseEffect;

		// 인스턴스 메서드
		public LifeCycleStateMachine(UObject Target) {
			target = Target ?? throw new ArgumentNullException(nameof(Target));
		}

		private void CheckConfiguration(LifeCycleState State) {
			// OnFirstGet 전에 실행하도록 강제함
			if (isPrepared) throw new InvalidOperationException("Life cycle states can only be configured before OnFirstGet completed.");
			if (State == null || !ReferenceEquals(State.target, target))
				throw new ArgumentException("State have wrong target", nameof(State));
		}
		
		public void SetGettingState(Getting State) { CheckConfiguration(State); GettingState = State; }
		public void SetReleasingState(Releasing State) { CheckConfiguration(State); ReleasingState = State; }
		public void SetStates(Getting GettingState, Releasing ReleasingState) {
			SetGettingState(GettingState);
			SetReleasingState(ReleasingState);
		}
		
		public void SetDefaultStates(Getting GettingState = null, Releasing ReleasingState = null) {
			// OnFirstGet 전에 실행하도록 강제함
			if (isPrepared) throw new InvalidOperationException("Life cycle is already prepared.");
			if (GettingState != null) { CheckConfiguration(GettingState); this.GettingState ??= GettingState; }
			if (ReleasingState != null) { CheckConfiguration(ReleasingState); this.ReleasingState ??= ReleasingState; }
		}

		public void Prepare() {
			if (isPrepared) return;
			SetDefaultStates(new DefaultGetting(target), new DefaultReleasing(target));
			isPrepared = true;
		}

		public void Get(bool PlayEffect) {
			if (!isPrepared || isFaulted || isShuttingDown || phase != LifeCyclePhase.Released || Completing)
				throw new InvalidOperationException("Only prepared, released UObject can begin new life.");
			
			lifeNumber++;
			ReleaseStarted = false;
			InstantReleaseRequested = false;
			
			phase = LifeCyclePhase.Getting;
			PrepareState(GettingState, PlayEffect);
			
			long Execution = executionNumber;
			long Life = lifeNumber;
			try {
				
				target.PrepareGetting();
				
				if (!IsCurrent(GettingState, Execution)) return;
				StartState(PlayEffect);
			}
			catch (Exception Error) { Fail(Error, Life); }
		}

		public void Release(bool PlayEffect = true) {
			
			if (Entering && !isShuttingDown && !isFaulted && !Completing) {
				
				if (phase == LifeCyclePhase.Releasing && PlayEffect) return;
				PendingRelease = true;
				
				PendingReleaseEffect &= PlayEffect;
				return;
			}
			
			if (UObject.isStoppingLifeCycles) { Shutdown(); return; }
			
			if (isShuttingDown || isFaulted || Completing || phase == LifeCyclePhase.Released) return;
			
			if (!target.gameObject.activeInHierarchy) { Shutdown(); return; }
			if (phase == LifeCyclePhase.Releasing) {
				if (PlayEffect) return;
				
				if (Starting) { InstantReleaseRequested = true; return; }
				RequestComplete(currentState, executionNumber);
				return;
			}

			long Life = lifeNumber;
			
			LifeCycleState Previous = currentState;
			long PreviousExecution = executionNumber;
			
			phase = LifeCyclePhase.Releasing;
			Starting = true;
			try {
				if (Previous != null) {
					
					Previous.Exit();
					
					target.UnregisterLifeCycle(PreviousExecution);
				}
				
				if (isShuttingDown) { Starting = false; return; }
				PrepareState(ReleasingState, PlayEffect);
				
				target.PrepareReleasing();
				Starting = false;
				if (isShuttingDown || isFaulted) return;
				
				StartState(PlayEffect && !InstantReleaseRequested);
			}
			catch (Exception Error) { Starting = false; Fail(Error, Life); }
		}
		
		public void MarkReleaseStarted() => ReleaseStarted = true;

		private void PrepareState(LifeCycleState State, bool PlayEffect) {
			currentState = State;
			executionNumber++;
			State.PrepareExecution(this, executionNumber, PlayEffect);
		}

		private void StartState(bool PlayEffect) {
			LifeCycleState State = currentState;
			long Execution = executionNumber;
			
			if (!PlayEffect) {
				RequestComplete(State, Execution);
				return;
			}
			
			target.RegisterLifeCycle(Execution);
			
			bool CompleteRequested = false;
			bool ReleaseRequested = false;
			bool ReleaseEffect = true;
			
			Entering = true;
			PendingCompletion = false;
			PendingRelease = false;
			PendingReleaseEffect = true;
			
			try {
				State.Enter();
				
				CompleteRequested = PendingCompletion;
				ReleaseRequested = PendingRelease;
				ReleaseEffect = PendingReleaseEffect;
			}
			finally {
				Entering = false;
				PendingCompletion = false;
				PendingRelease = false;
				PendingReleaseEffect = false;
			}
			
			if (!IsCurrent(State, Execution)) return;
			
			if (ReleaseRequested) { Release(ReleaseEffect); return; }
			if (CompleteRequested) RequestComplete(State, Execution);
		}

		public bool IsCurrent(LifeCycleState State, long Execution) {
			bool exit             = isFaulted || isShuttingDown;
			bool isRightState     = ReferenceEquals(currentState, State);
			bool isRightExecution = Execution == executionNumber;
			bool isRightPhase     = phase is LifeCyclePhase.Getting or LifeCyclePhase.Releasing;
			
			return !exit && isRightState && isRightExecution && isRightPhase;
		}

		public void Tick(long Execution, float DeltaTime) {
			if (Completing || Starting || Entering || !IsCurrent(currentState, Execution)) return;
			long Life = lifeNumber;
			
			try { currentState.Tick(DeltaTime); }
			catch (Exception Error) { Fail(Error, Life); }
		}

		public void RequestComplete(LifeCycleState State, long Execution) {
			if (State == null || Completing || !IsCurrent(State, Execution)) return;
			
			if (Entering) { PendingCompletion = true; return; }
			if (Starting) return;
			long Life = lifeNumber;
			
			bool WasGetting = phase == LifeCyclePhase.Getting;
			Completing = true;
			
			try {
				State.MarkComplete();
				
				State.ClearEffectOnce();
				if (isShuttingDown) return;
				
				State.Exit();
				if (isShuttingDown) return;
				
				currentState = null;
				target.UnregisterLifeCycle(Execution);
				
				if (WasGetting) {
					phase = LifeCyclePhase.Active;
					target.ActivateLife();
					Completing = false;

					target.Initialize();
				}
				else {
					target.FinishReleasing();
					if (isShuttingDown) return;
					phase      = LifeCyclePhase.Released;
					Completing = false;

					target.ReturnToPool();
				}
			}
			catch (Exception Error) {
				Fail(Error, Life);
			}
		}

		private void Fail(Exception Error, long Life) {
			if (lifeNumber == Life) {
				isFaulted = true;
				Shutdown();
			}
			
			DebugManager.LogException(Error, target);
		}

		public void Shutdown() {
			if (isShuttingDown) return;
			isShuttingDown = true;
			
			Completing = true;
			
			if (phase != LifeCyclePhase.Released) phase = LifeCyclePhase.Releasing;
			
			LifeCycleState State = currentState;
			currentState = null;
			target.UnregisterLifeCycle(executionNumber);
			target.DeactivateLife();
			
			try { State?.Exit(); }
			catch (Exception Error) { DebugManager.LogException(Error, target); }
			
			
			try { target.StopLife(ReleaseStarted); }
			catch (Exception Error) { DebugManager.LogException(Error, target); }
			phase = LifeCyclePhase.Released;
			
			target.QuarantineLife();
		}
	}
}
