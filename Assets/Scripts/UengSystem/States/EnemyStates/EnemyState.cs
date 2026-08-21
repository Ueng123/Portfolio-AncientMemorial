using AncientMemorial.Entities;
using UengSystem.Utility;
using UnityEngine.PlayerLoop;

namespace UengSystem.States.EnemyStates {
	public abstract class EnemyState : UState {
		public Enemy enemy;
		protected EnemyStateMachine stateMachine;

		public EnemyState Init(EnemyStateMachine stateMachine) {
			this.stateMachine = stateMachine;
			enemy             = stateMachine.enemy;
			return this;
		}
		
		protected EnemyState GetState() {
			Entity target = stateMachine.GetTarget.Invoke();
			if (!target) return stateMachine.wanderState;

			bool canAttack = stateMachine.CanAttack();
			if (!canAttack) return stateMachine.awareState;

			return stateMachine.attackReadyState;
		}
		
		public override void OnEnter() { }
		
		public override void OnEarlyRoutine() {
			if (enemy.state != GetState()) enemy.state = GetState();
		}

		public override void OnRoutine() { }

		public override void OnLateRoutine() { }

		public override void OnFixedRoutine() { }

		public override void OnExit() { }
	}
}