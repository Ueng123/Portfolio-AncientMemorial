using System;
using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using UengSystem.Utility;

namespace UengSystem.States.EnemyStates {
	public class EnemyStateMachine {
		public Enemy       enemy;
		public Func<Entity> GetTarget;
		
		private   StopWatch attackWatch = new StopWatch();
		protected float     attackDelay => enemy.entityData.attackCooldown;

		public bool CanAttack() {
			return !attackWatch.Check(attackDelay);
		}

		public void AttackWatchTick() {
			attackWatch.Tick();
		}
		
		public void Attack() {
			enemy.Attack();
		}
		
		public EnemyState wanderState;
		public EnemyState awareState;
		public EnemyState attackReadyState;
		public EnemyStun  stunState;
		
		public EnemyStateMachine(Enemy enemy, Func<Entity> getTarget, EnemyState wanderState, EnemyState awareState, EnemyState attackReadyState, EnemyStun stunState) {
			this.enemy            = enemy;
			GetTarget             = getTarget;
			
			this.wanderState      = wanderState;
			this.awareState       = awareState;
			this.attackReadyState = attackReadyState;
			this.stunState        = stunState;
			
			wanderState.Init(this);
			awareState.Init(this);
			attackReadyState.Init(this);
			stunState.Init(this);
			
			attackWatch.Tick();
		}
	}
}