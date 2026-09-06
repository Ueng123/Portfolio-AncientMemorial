using System;
using UengSystem.States;

namespace UengSystem.Objects.LifeCycle {
	public abstract class LifeCycleState : UState {
		public UObject target { get; }
		public float elapsed { get; private set; }
		public long executionNumber { get; private set; }
		public bool isComplete { get; private set; }
		public bool isEffectCleared { get; private set; }
		public bool playEffect { get; private set; }
		protected bool hasStartedEffect { get; private set; }
		protected virtual float duration => 0;
		private LifeCycleStateMachine Machine;
		private long CallbackExecution;
		private bool HasExited;

		protected LifeCycleState(UObject Target) {
			target = Target ?? throw new ArgumentNullException(nameof(Target));
		}

		internal void PrepareExecution(LifeCycleStateMachine Machine, long Execution, bool PlayEffect) {
			this.Machine = Machine;
			executionNumber = Execution;
			playEffect = PlayEffect;
			elapsed = 0;
			isComplete = false;
			isEffectCleared = false;
			hasStartedEffect = false;
			CallbackExecution = 0;
			HasExited = false;
		}

		public sealed override void OnEnter() {
			if (!playEffect) return;
			hasStartedEffect = true;
			InvokeEffect(OnStartEffect);
		}

		internal void Tick(float DeltaTime) {
			if (isComplete || !Machine.IsCurrent(this, executionNumber)) return;
			elapsed += DeltaTime;
			InvokeEffect(() => {
				if (elapsed >= duration) {
					Complete();
					return;
				}
				OnEffectRoutine(DeltaTime);
			});
		}

		private void InvokeEffect(Action Callback) {
			long Execution = executionNumber;
			CallbackExecution = Execution;
			try { Callback(); }
			finally {
				// The callback may return this object and synchronously acquire it again.
				if (Machine.IsCurrent(this, Execution)) CallbackExecution = 0;
			}
		}

		// No-argument completion is valid only inside the current synchronous effect callback.
		// Delayed work must capture its execution with CaptureCompletion().
		protected void Complete() {
			if (CallbackExecution != 0) Machine.RequestComplete(this, CallbackExecution);
		}

		protected Action CaptureCompletion() {
			long Execution = executionNumber;
			LifeCycleStateMachine CurrentMachine = Machine;
			return () => CurrentMachine.RequestComplete(this, Execution);
		}

		internal void MarkComplete() => isComplete = true;

		internal void ClearEffectOnce() {
			if (isEffectCleared) return;
			// Mark before invoking user code, including throwing/reentrant cleanup.
			isEffectCleared = true;
			ClearEffect();
		}

		public sealed override void OnExit() {
			if (HasExited) return;
			HasExited = true;
			ClearEffectOnce();
			OnStateExit();
		}

		protected virtual void OnStartEffect() { }
		protected virtual void OnEffectRoutine(float DeltaTime) { }
		protected virtual void ClearEffect() { }
		protected virtual void OnStateExit() { }
		public sealed override void OnEarlyRoutine() { }
		public sealed override void OnRoutine() { }
		public sealed override void OnLateRoutine() { }
		public sealed override void OnFixedRoutine() { }
	}

	public abstract class Getting : LifeCycleState {
		protected Getting(UObject Target) : base(Target) { }
		protected override float duration => target.gettingDuration;
	}

	public abstract class Releasing : LifeCycleState {
		protected Releasing(UObject Target) : base(Target) { }
		protected override float duration => target.releasingDuration;
	}
}
