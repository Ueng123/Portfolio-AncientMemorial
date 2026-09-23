using System;
using UengSystem.States;

namespace UengSystem.Objects.LifeCycle {
	public abstract class LifeCycleState : UState {

		// 인스턴스 프로퍼티
		public UObject target { get; }
		private LifeCycleStateMachine Machine;
		
		public float elapsed { get; private set; }
		protected virtual float duration => 0;
		
		public long executionNumber { get; private set; }
		
		public bool isComplete { get; private set; }
		
		public bool isEffectCleared { get; private set; }
		public bool playEffect { get; private set; }
		protected bool hasStartedEffect { get; private set; }
		
		private bool HasExited;

		// 인스턴스 메서드
		protected LifeCycleState(UObject Target) {
			target = Target
		}

		public void PrepareExecution(LifeCycleStateMachine Machine, long Execution, bool PlayEffect) {
			this.Machine = Machine;
			executionNumber = Execution;
			playEffect = PlayEffect;
			elapsed = 0;
			isComplete = false;
			isEffectCleared = false;
			hasStartedEffect = false;
			HasExited = false;
		}

		public void Tick(float DeltaTime) {
			if (isComplete || !Machine.IsCurrent(this, executionNumber)) return;
			elapsed += DeltaTime;

			// 유효성 검사
			if (elapsed >= duration) {
				Machine.RequestComplete(this, executionNumber);
				return;
			}
			
			OnEffectRoutine(DeltaTime);
		}

		public void MarkComplete() => isComplete = true;

		public void ClearEffectOnce() {
			if (isEffectCleared) return;
			isEffectCleared = true;
			
			ClearEffect();
		}

		protected virtual void OnStartEffect() { }
		protected virtual void OnEffectRoutine(float DeltaTime) { }
		protected virtual void ClearEffect() { }
		protected virtual void OnStateExit() { }

		// 오버라이드 메서드
		public override void OnEnter() {
			if (!playEffect) return;
			hasStartedEffect = true;
			OnStartEffect();
		}

		public override void OnExit() {
			if (HasExited) return;
			HasExited = true;
			ClearEffectOnce();
			OnStateExit();
		}
		
		public override void OnEarlyRoutine() { }
		public override void OnRoutine() { }
		public override void OnLateRoutine() { }
		public override void OnFixedRoutine() { }
	}
}
