using System;

namespace UengSystem.Objects.LifeCycle {
	public sealed class LifeCycleStateMachine {
		public UObject target { get; }
		public LifeCyclePhase phase { get; private set; } = LifeCyclePhase.Released;
		public LifeCycleState currentState { get; private set; }
		public long lifeNumber { get; private set; }
		public long executionNumber { get; private set; }
		public bool isPrepared { get; private set; }
		public bool isFaulted { get; private set; }
		public bool isShuttingDown { get; private set; }
		private Getting GettingState;
		private Releasing ReleasingState;
		private bool Completing;
		private bool Starting;
		private bool InstantReleaseRequested;
		private bool ReleaseStarted;

		public LifeCycleStateMachine(UObject Target) {
			target = Target ?? throw new ArgumentNullException(nameof(Target));
		}

		private void CheckConfiguration(LifeCycleState State) {
			if (isPrepared) throw new InvalidOperationException("Life cycle states can only be configured before OnFirstGet completes.");
			if (State == null || !ReferenceEquals(State.target, target))
				throw new ArgumentException("Each life cycle state must belong to this UObject.", nameof(State));
		}

		public void SetGettingState(Getting State) { CheckConfiguration(State); GettingState = State; }
		public void SetReleasingState(Releasing State) { CheckConfiguration(State); ReleasingState = State; }
		public void SetStates(Getting GettingState, Releasing ReleasingState) {
			CheckConfiguration(GettingState);
			CheckConfiguration(ReleasingState);
			this.GettingState = GettingState;
			this.ReleasingState = ReleasingState;
		}
		public void SetDefaultStates(Getting GettingState = null, Releasing ReleasingState = null) {
			if (isPrepared) throw new InvalidOperationException("Life cycle is already prepared.");
			if (GettingState != null) { CheckConfiguration(GettingState); this.GettingState ??= GettingState; }
			if (ReleasingState != null) { CheckConfiguration(ReleasingState); this.ReleasingState ??= ReleasingState; }
		}

		internal void Prepare() {
			if (isPrepared) return;
			SetDefaultStates(new DefaultGetting(target), new DefaultReleasing(target));
			isPrepared = true;
		}

		internal void Get(bool PlayEffect) {
			if (!isPrepared || isFaulted || isShuttingDown || phase != LifeCyclePhase.Released || Completing)
				throw new InvalidOperationException("Only a prepared, released UObject can begin a new life.");
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
			if (UObject.isStoppingLifeCycles) { Shutdown(); return; }
			if (isShuttingDown || isFaulted || Completing || phase == LifeCyclePhase.Released) return;
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
					Previous.Exit(); // Cancellation never invokes Initialize.
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

		private void PrepareState(LifeCycleState State, bool PlayEffect) {
			currentState = State;
			executionNumber++;
			State.PrepareExecution(this, executionNumber, PlayEffect);
		}

		internal void MarkReleaseStarted() => ReleaseStarted = true;

		private void StartState(bool PlayEffect) {
			LifeCycleState State = currentState;
			long Execution = executionNumber;
			if (!PlayEffect) {
				RequestComplete(State, Execution);
				return;
			}
			target.RegisterLifeCycle(Execution);
			State.Enter();
		}

		internal bool IsCurrent(LifeCycleState State, long Execution) {
			return !isFaulted && !isShuttingDown && ReferenceEquals(currentState, State)
			       && Execution == executionNumber && (phase == LifeCyclePhase.Getting || phase == LifeCyclePhase.Releasing);
		}

		internal void Tick(long Execution, float DeltaTime) {
			if (Completing || Starting || !IsCurrent(currentState, Execution)) return;
			long Life = lifeNumber;
			try { currentState.Tick(DeltaTime); }
			catch (Exception Error) { Fail(Error, Life); }
		}

		internal void RequestComplete(LifeCycleState State, long Execution) {
			if (State == null || Completing || Starting || !IsCurrent(State, Execution)) return;
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
					target.Initialize(); // No writes after this hook: it may release/reacquire.
					return;
				}
				target.FinishReleasing(); // Includes Uninitialize; requests remain blocked.
				if (isShuttingDown) return;
				phase = LifeCyclePhase.Released;
				Completing = false;
				target.ReturnToPool(); // Last operation on this life.
			}
			catch (Exception Error) { Fail(Error, Life); }
		}

		private void Fail(Exception Error, long Life) {
			if (lifeNumber == Life) {
				isFaulted = true;
				Shutdown();
			}
			target.ReportLifeCycleError(Error);
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
			catch (Exception Error) { target.ReportLifeCycleError(Error); }
			try { target.StopLife(ReleaseStarted); }
			catch (Exception Error) { target.ReportLifeCycleError(Error); }
			phase = LifeCyclePhase.Released;
			// Quarantine failures and scene objects. Never return to a destroying pool.
			target.QuarantineLife();
		}
	}
}
