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
		public abstract EnemyState GetWander();
		public abstract EnemyState GetAware();
		public abstract EnemyState GetAttackReady();
		public abstract EnemyState GetAttack();
		
		public float markYPos;
		
		private IEnumerator StunEnumerator(float duration) {
			OnStunStartHandler();
			yield return new WaitForSeconds(duration);
		}

		private GameObject stunEffect;
		public void OnStunStartHandler() {
			OnStunStart();
			stunEffect = UObjectPool.instance.Get("StunEffect", transform.position + Vector3.up * markYPos);
			stunEffect.transform.SetParent(transform);
		}

		public void OnStunEndHandler() {
			OnStunEnd();
			UObjectPool.instance.Release(stunEffect);
			stunEffect = null;
		}
		public abstract void OnStunStart();
		public abstract void OnStunEnd();
		
		public UState Stun(float duration) {
			// return new UState(StunEnumerator(duration), OnStunEndHandler, OnStunEndHandler, 0, this);
			return null;
		}

		public static bool isTargettable(EntityType entity) => entity == EntityType.Player;

		public override void Initialize() {
			base.Initialize();
			attackable = false;
			state      = GetWander();
		}

		private WaitForSeconds waitTime1;
		private WaitForSeconds waitTime2;
		private WaitForSeconds waitTime3;

		protected StopWatch attackableTimer = new ();
		private   float     attackableCoeff;
		protected bool attackable {
			get => !attackableTimer.Check(entityData.attackCooldown * attackableCoeff);
			set {
				if (value) return;
				attackableCoeff = Random.Range(1f, 1.1f);
				attackableTimer.Tick();
			}
		}
		
		protected abstract IEnumerator   AttackEnumerator();
		protected abstract void          OnAttackDone();
		protected abstract void          OnAttackCancel();
		public virtual void Attack() {
			if (!attackable) return;
			attackable = false;
			
			// ((UObject)this).state = Attacking;
		}
		
		// public UState Attacking    => new (AttackEnumerator(), () => {
		// 	attackable = false;
		// 	OnAttackCancel();
		// }, () => {
		// 	attackable = false;
		// 	OnAttackDone();
		// }, 0, this);
		
		public override bool isAttackTarget(Entity entity) {
			return entity != this && entity == player;
		} 

		protected override void OnHeal(float amount) {
			ShowDamageUI(amount);
		}

		
		protected override void Death() {
			GameManager.UValueFloatVariables["EnemyDead"] = new UPureFloat {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
			if (Category == "WaveEnemy") GameManager.UValueFloatVariables["WaveEnemyDead"]   = new UPureFloat {number = GameManager.UValueFloatVariables["WaveEnemyDead"].value + 1};
			
			base.Death();
		}
	}
}