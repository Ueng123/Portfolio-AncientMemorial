using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using UengSystem.States;

namespace AncientMemorial.States.EnemyStates {
	public abstract class EnemyState : UState {

		// 인스턴스 프로퍼티
		public Enemy enemy;
		protected EnemyStateMachine stateMachine;

		// 인스턴스 메서드
		public virtual EnemyState Init(EnemyStateMachine stateMachine) {
			this.stateMachine = stateMachine;
			enemy             = stateMachine.enemy;
			return this;
		}
		
		protected virtual EnemyState GetState() {
			Entity target = stateMachine.getTarget.Invoke();
			if (!target) return stateMachine.wanderState;

			bool canAttack = stateMachine.CanAttack();
			if (!canAttack) return stateMachine.awareState;

			return stateMachine.attackReadyState;
		}

		// 오버라이드 메서드
		public override void OnEnter() { }
		
		public override void OnEarlyRoutine() {
			if (enemy.entityState != GetState()) enemy.entityState = GetState();
		}

		public override void OnRoutine() { }

		public override void OnLateRoutine() { }

		public override void OnFixedRoutine() { }

		public override void OnExit() { }
	}
}