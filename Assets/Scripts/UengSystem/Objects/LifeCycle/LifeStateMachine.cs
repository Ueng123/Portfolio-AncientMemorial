using System;
using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class LifeStateMachine {

		// 인스턴스 프로퍼티
		public UObject target { get; }
		
		public LifePhase phase = LifePhase.Released;
		public LifeState currentState;
		
		public int lifeNumber;
		public int executionNumber;
		
		public bool isPrepared;
		public bool isShuttingDown;
		
		private Getting GettingState;
		private Releasing ReleasingState;
		
		private bool CompleteWorking;
		private bool ReleaseWorking;
		private bool StateStarting;

		public bool ReleaseStarted;
		
		private bool PendingComplete;
		private bool PendingRelease;
		private bool PendingReleaseEffect;
		
		private bool InstantReleaseRequested;

		// 인스턴스 메서드
		public LifeStateMachine(UObject Target) {
			target = Target ?? throw new ArgumentNullException(nameof(Target));
		}

		private void CheckConfiguration(LifeState State) {
			// OnFirstGet 전에 실행하도록 강제함
			if (isPrepared) throw new InvalidOperationException("Life cycle states can only be configured before OnFirstGet completed.");
			
			if (!ReferenceEquals(State.target, target)) throw new ArgumentException("State have wrong target", nameof(State));
		}
		
		public void SetGettingState(Getting State) {
			CheckConfiguration(State);
			GettingState = State;
		}

		public void SetReleasingState(Releasing State) {
			CheckConfiguration(State);
			ReleasingState = State;
		}

		public void SetStates(Getting GettingState, Releasing ReleasingState) {
			SetGettingState(GettingState);
			SetReleasingState(ReleasingState);
		}
		
		public void SetDefaultStates(Getting GettingState = null, Releasing ReleasingState = null) {
			if (isPrepared) throw new InvalidOperationException("Life cycle is already prepared.");

			if (GettingState != null) {
				CheckConfiguration(GettingState);
				this.GettingState ??= GettingState;
			}

			if (ReleasingState != null) {
				CheckConfiguration(ReleasingState);
				this.ReleasingState ??= ReleasingState;
			}
		}

		public void Prepare() {
			if (isPrepared) return;

			SetDefaultStates(new DefaultGetting(target), new DefaultReleasing(target));

			isPrepared = true;
		}

		public void Get(bool PlayEffect) {
			if (!isPrepared) throw new InvalidOperationException("Object Not Prepared");
			if(isShuttingDown) throw new InvalidOperationException("Object shutting down");
			if(phase != LifePhase.Released)throw new InvalidOperationException("Object Not Released");
			if(CompleteWorking) throw new InvalidOperationException("Object is Completing");
			
			lifeNumber++;
			ReleaseStarted = false;
			InstantReleaseRequested = false;
			
			phase = LifePhase.Getting;
			SetState(GettingState, PlayEffect);
			
			int Execution = executionNumber;

			target.PrepareGetting();
			
			if (!IsCurrent(GettingState, Execution)) return;

			StartState(PlayEffect);
		}

		public void Release(bool PlayEffect = true) {
			if (StateStarting && !isShuttingDown && !CompleteWorking) {
				if (phase == LifePhase.Releasing && PlayEffect) return;

				PendingRelease = true;
				PendingReleaseEffect &= PlayEffect;
				return;
			}
			
			if (UObject.isStoppingLifeCycles) {
				Shutdown();
				return;
			}

			if (isShuttingDown) return;
			if (CompleteWorking) return;
			if (phase == LifePhase.Released) return;
			
			if (!target.gameObject.activeInHierarchy) PlayEffect = false;

			if (phase == LifePhase.Releasing && ReleaseWorking) {
				InstantReleaseRequested = true;
				return;
			}
			
			phase = LifePhase.Releasing;
			ReleaseWorking = true;

			if (currentState != null) {
				int currExecution = executionNumber;
				currentState.Exit();
				target.EndLifeRoutine(currExecution);
			}
			
			if (isShuttingDown) {
				ReleaseWorking = false;
				return;
			}

			SetState(ReleasingState, PlayEffect);
			
			target.PrepareReleasing();
			ReleaseWorking = false;

			if (isShuttingDown) return;
			
			StartState(PlayEffect && !InstantReleaseRequested);
		}

		private void SetState(LifeState State, bool PlayEffect) {
			currentState = State;
			executionNumber++;

			State.Initialize(this, executionNumber, PlayEffect);
		}

		private void StartState(bool PlayEffect) {
			LifeState usingState = currentState;
			int usingExecutionNumber = executionNumber;
			
			if (!PlayEffect) {
				Complete(usingState, usingExecutionNumber);
				return;
			}
			
			target.StartLifeRoutine(usingExecutionNumber);

			StateStarting = true;
			PendingComplete = false;
			PendingRelease = false;
			PendingReleaseEffect = true;
			
			usingState.Enter();
			bool CompleteRequested = PendingComplete;
			bool ReleaseRequested = PendingRelease;
			bool ReleaseEffect = PendingReleaseEffect;

			StateStarting = false;
			PendingComplete = false;
			PendingRelease = false;
			PendingReleaseEffect = false;
			
			if (!IsCurrent(usingState, usingExecutionNumber)) return;
			
			if (ReleaseRequested) {
				Release(ReleaseEffect);
				return;
			}

			if (CompleteRequested) {
				Complete(usingState, usingExecutionNumber);
				return;
			}
		}

		public bool IsCurrent(LifeState State, int Execution) {
			bool exit = isShuttingDown;
			bool isRightState = ReferenceEquals(currentState, State);
			bool isRightExecution = Execution == executionNumber;
			bool isRightPhase = phase is LifePhase.Getting or LifePhase.Releasing;
			
			return !exit && isRightState && isRightExecution && isRightPhase;
		}

		public void EffectRoutine(int Execution, float DeltaTime) {
			if (CompleteWorking) return;
			if (ReleaseWorking) return;
			if (StateStarting) return;
			if (!IsCurrent(currentState, Execution)) return;
			
			currentState.EffectRoutine(DeltaTime);
		}

		public void Complete(LifeState State, int Execution) {
			if (State == null) return;
			if (!IsCurrent(State, Execution)) return;
			
			if (StateStarting) { PendingComplete = true; return; }
			if (ReleaseWorking) return;
			if (CompleteWorking) return;
			
			bool WasGetting = phase == LifePhase.Getting;
			CompleteWorking = true;

			State.completed = true;
			State.Exit();

			if (isShuttingDown) return;
			
			currentState = null;
			target.EndLifeRoutine(Execution);
			
			if (WasGetting) {
				phase = LifePhase.Active;
				target.ActivateLife();

				CompleteWorking = false;

				target.Initialize();
			}
			else {
				target.FinishReleasing();

				if (isShuttingDown) return;

				phase = LifePhase.Released;
				CompleteWorking = false;

				target.ReturnToPool();
			}
		}

		public void Shutdown() {
			if (isShuttingDown) return;

			isShuttingDown = true;
			CompleteWorking = true;
			
			phase = LifePhase.Releasing;
			
			LifeState State = currentState;
			currentState = null;

			target.EndLifeRoutine(executionNumber);
			target.DeactivateLife();
			
			State?.Exit();
			target.StopLife(ReleaseStarted);

			phase = LifePhase.Released;
			
			target.gameObject.SetActive(false);
		}
	}
}
