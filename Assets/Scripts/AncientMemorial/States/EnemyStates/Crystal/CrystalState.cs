using System.Collections.Generic;
using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Crystal {
	public abstract class CrystalState : EnemyState {
		public CrystalBoss crystal;
		
		public override EnemyState Init(EnemyStateMachine stateMachine) {
			base.Init(stateMachine);
			crystal = enemy.To<CrystalBoss>();
			return this;
		}
		
		protected void ShootMissile(Vector2 pos, float rot, float speed = 13.5f, float timeBeforeLockOn = 0, float lockOnDuration = 0f) {
			GameObject missile = UObjectPool.instance.Get("CrystalMissile", pos);
			
			CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
			crystalMissile.owner              = crystal;
			crystalMissile.damage             = crystal.stat.attackDamage;
			crystalMissile.offset             = 0;
			crystalMissile.TimeBeforeLockOn   = timeBeforeLockOn;
			crystalMissile.LockOnDuration     = lockOnDuration;
			crystalMissile.initialSpeed       = speed;
			crystalMissile.transform.rotation = Quaternion.Euler(0, 0, rot);
			crystalMissile.Category           = "crystalMissile";
		}
		
		protected void ShootBigMissile(Vector2 pos, float rot, float speed = 13.5f, float timeBeforeLockOn = 0, float lockOnDuration = 0f) {
			GameObject missile = UObjectPool.instance.Get("CrystalBigMissile", pos);
			
			CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
			crystalMissile.owner              = crystal;
			crystalMissile.damage             = crystal.stat.attackDamage;
			crystalMissile.offset             = 0;
			crystalMissile.TimeBeforeLockOn   = timeBeforeLockOn;
			crystalMissile.LockOnDuration     = lockOnDuration;
			crystalMissile.initialSpeed       = speed;
			crystalMissile.transform.rotation = Quaternion.Euler(0, 0, rot);
			crystalMissile.Category           = "crystalMissile";
		}

		protected virtual void ClearMissiles() {
			List<UObject> missiles = UObject.GetUObjects("crystalMissile");
			for (int i = missiles.Count - 1; i >= 0; i--) {
				UObject missile = missiles[i];
				UObjectPool.instance.Release(missile.gameObject);
			}
		}

		public override void OnExit() {
			ClearMissiles();
		}
	}
}