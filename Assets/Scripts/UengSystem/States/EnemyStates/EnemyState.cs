using AncientMemorial.Entities;
using UengSystem.Utility;

namespace UengSystem.States.EnemyStates {
	public abstract class EnemyState : UState {
		protected Enemy  enemy;
		protected abstract Entity target { get; }
		
		private   StopWatch attackWatch = new StopWatch();
		protected float attackDelay => enemy.entityData.attackCooldown;

		protected bool CanAttack() {
			return !attackWatch.Check(attackDelay);
		}

		protected EnemyState GetState() {
			if (!target) return enemy.GetWander();

			bool canAttack = CanAttack();
			if (!canAttack) return enemy.GetAware();

			return enemy.GetAttackReady();
		}
		
		public override void OnEnter() {
			enemy = (Enemy)owner;
		}
		
		public override void OnEarlyRoutine() {
			if (enemy.state != GetState()) enemy.state = GetState();
		}

		public override void OnRoutine() {
			
		}

		public override void OnLateRoutine() {
			
		}

		public override void OnFixedRoutine() {
			
		}

		public override void OnExit() {
			
		}
	}
}