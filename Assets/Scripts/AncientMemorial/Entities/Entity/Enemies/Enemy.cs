using AncientMemorial.States.EnemyStates;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;

// using UengSystem.VisualScripting.UValues.UFloats;

namespace AncientMemorial.Entities.Enemies {
	public abstract class Enemy : Entity {

		private readonly int ENEMY_DEAD = "EnemyDead".GetHash();
		private          int ENEMYTYPE_DEAD;
		private          int CATEGORY_DEAD;
		
		public float markYPos;

		protected EnemyStateMachine stateMachine;

		protected abstract Entity GetTargetEntity();
		public abstract void  Attack();
		
		public void Stun(float duration) {
			EnemyStun stunState = stateMachine.stunState;
			entityState = stunState.Setup(duration);
		}

		public static bool isTargettable(EntityType entity) => entity == EntityType.Player;
		
		public override bool isAttackTarget(Entity entity) {
			return entity != this && entity == player;
		} 

		protected override void OnHeal(float amount) {
			ShowDamageUI(amount);
		}

		protected void InitializeStateMachine(EnemyState wanderState, EnemyState awareState, EnemyState attackReadyState, EnemyStun stunState = null) {
			stateMachine = new EnemyStateMachine(this, GetTargetEntity, wanderState, awareState, attackReadyState, stunState??new EnemyStun());
		}
		
		protected override void Death() {
			UPureFloat.AddValue(ENEMY_DEAD, 1);
			UPureFloat.AddValue(ENEMYTYPE_DEAD, 1);
			if (!string.IsNullOrWhiteSpace(Category)) {
				UPureFloat.AddValue(CATEGORY_DEAD, 1);
			}
			
			base.Death();
		}

		public override void OnFirstGet() {
			base.OnFirstGet();
			ENEMYTYPE_DEAD = $"{GetType().Name}Dead".GetHash();
		}

		public virtual void InitializeState() {
			entityState = stateMachine?.wanderState;
		}
		
		public override void Initialize() {
			base.Initialize();
			InitializeState();
		}

		protected override void OnCategoryChanged(string category) {
			if (!string.IsNullOrWhiteSpace(category)) {
				CATEGORY_DEAD = $"{category}Dead".GetHash();
			}
		}
	}
}
