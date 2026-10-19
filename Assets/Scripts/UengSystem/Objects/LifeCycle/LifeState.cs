using System;
using UengSystem.States;

namespace UengSystem.Objects.LifeCycle {
	public abstract class LifeState : UState {

		// 인스턴스 프로퍼티
		public UObject target { get; }
		private LifeStateMachine Machine;

		protected         float elapsed ;
		protected virtual float duration => 0;
		
		public int executionNumber;
		
		public bool completed;
		
		public bool effectCleared;
		public bool playEffect;
		protected bool entered;
		
		private bool exited;

		// 인스턴스 메서드
		protected LifeState(UObject Target) {
			target = Target;
		}

		public void Initialize(LifeStateMachine Machine, int Execution, bool PlayEffect) {
			this.Machine = Machine;
			executionNumber = Execution;
			playEffect = PlayEffect;
			elapsed = 0;
			completed = false;
			effectCleared = false;
			entered = false;
			exited = false;
		}

		public void EffectRoutine(float DeltaTime) {
			if (completed) return;
			if (!Machine.IsCurrent(this, executionNumber)) return;
			
			elapsed += DeltaTime;

			// 유효성 검사
			if (elapsed >= duration) {
				Machine.Complete(this, executionNumber);
				return;
			}
			
			OnEffectRoutine(DeltaTime);
		}

		public void ClearEffectOnce() {
			if (effectCleared) return;
			effectCleared = true;
			
			ClearEffect();
		}

		protected virtual void OnEffectRoutine(float DeltaTime) { }
		protected virtual void ClearEffect() { }

		// 오버라이드 메서드
		public override void OnEnter() {
			if (!playEffect) return;
			entered = true;
		}

		public override void OnExit() {
			if (exited) return;
			exited = true;
			
			ClearEffectOnce();
		}
		
		public override void OnEarlyRoutine() { }
		public override void OnRoutine() { }
		public override void OnLateRoutine() { }
		public override void OnFixedRoutine() { }
	}
}
