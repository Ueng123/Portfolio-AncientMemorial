using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using UengSystem.States;

namespace AncientMemorial.States.EnemyStates {
	public abstract class EnemyState : UState {
		public Enemy enemy;
		protected EnemyStateMachine stateMachine;

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