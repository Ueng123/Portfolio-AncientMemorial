using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States;
using UengSystem.States.EnemyStates;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public abstract class Enemy : Entity {
		
		public float markYPos;

		protected EnemyStateMachine stateMachine;
		
		public abstract Entity GetTargetEntity();
		public abstract void  Attack();
		
		public void Stun(float duration) {
			EnemyStun stunState = stateMachine.stunState;
			stunState.SetDuration(duration);
			
			state = stunState;
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
			GameManager.UValueFloatVariables["EnemyDead"] = new UPureFloat {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
			if (Category == "WaveEnemy") {
				GameManager.UValueFloatVariables["WaveEnemyDead"]   = new UPureFloat {number = GameManager.UValueFloatVariables["WaveEnemyDead"].value + 1};
			}
			
			base.Death();
		}
		
		public override void Initialize() {
			base.Initialize();
			state = stateMachine?.wanderState;
		}
	}
}