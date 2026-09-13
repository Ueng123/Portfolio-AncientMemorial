using System;
using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using UengSystem.Utility;

namespace AncientMemorial.States.EnemyStates {
	public class EnemyStateMachine {

		// 인스턴스 프로퍼티
		public Enemy       enemy;
		public Func<Entity> getTarget;
		
		private   StopWatch attackWatch = new StopWatch();
		protected float     attackDelay => enemy.data.attackCooldown;
		
		public EnemyState wanderState;
		public EnemyState awareState;
		public EnemyState attackReadyState;
		public EnemyStun  stunState;

		// 인스턴스 메서드
		public bool CanAttack() {
			return attackWatch.CheckOut(attackDelay);
		}

		public void AttackWatchTick() {
			attackWatch.Tick();
		}
		
		public void Attack() {
			enemy.Attack();
		}
		
		public EnemyStateMachine(Enemy enemy, Func<Entity> getTarget, EnemyState wanderState, EnemyState awareState, EnemyState attackReadyState, EnemyStun stunState) {
			this.enemy     = enemy;
			this.getTarget = getTarget;
			
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
