using AncientMemorial.Cameras;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities {
	public abstract class CrystalBoss : Enemy {
		protected override void        OnGrounded() { }

		protected bool groggy;
		protected bool groggyedEffect;
		
		public override    void        OnHit(Entity        attacker, float damage, Vector2? pushDir = null) {
			if (groggy) {
				ShowCriticalDamageUI(damage);
			}
			else {
				ShowDamageUI(damage);
			}
			
			if (entityStat.hp <= 0) {
				GameManager.SetTimeScale(0.5f, 0.25f);
				CameraBrain.instance.ShakeLerp(5, 1);
				CameraBrain.instance.ZoomLerp(-1f, 7.5f);
				return;
			}

			PlaySFX("crystalHit1");
			
			if (groggyedEffect) {
				groggyedEffect = false;
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraBrain.instance.ShakeLerp(2, 10);
				CameraBrain.instance.ZoomLerp(-0.1f);
				return;
			}
			
			if (groggy) {
				if (attacker != player) return;
				GameManager.SetTimeScale(0, 0.05f);
				CameraBrain.instance.ShakeLerp(1, 10);
				CameraBrain.instance.ZoomLerp(-0.1f, 20f);
				return;
			}
			
			if (attacker != player) return;
			GameManager.SetTimeScale(0, 0.05f);
			CameraBrain.instance.ShakeLerp(1, 10);
			CameraBrain.instance.ZoomLerp(-0.1f, 20f);
		}
		
		public override void OnStunStart() { }
		public override void OnStunEnd()   { }
		
		private DelayedAction EndGroggy;
		protected override float GetRealDamage(float rawDamage) {
			// if (groggyedEffect) {
			// 	groggyedEffect        = false;
			// 	((UObject)this).state = Stun(5);
			// 	state                 = EnemyState.Stun;
			// 	
			// 	EndGroggy ??= new DelayedAction(5, () => {
			// 		groggy = false;
			// 		state  = EnemyState.Alert;
			// 	});
			//
			// 	EndGroggy.ExecuteDA();
			// 	
			// 	return entityData.hp / 20f;
			// }
			//
			// if (groggy) return rawDamage*1.5f;
			return rawDamage;
		}

		protected void ShootMissile(Vector2 pos, float rot, float speed = 13.5f, float timeBeforeLockOn = 0.01f, float lockOnDuration = 0f) {
			GameObject missile = UObjectPool.instance.Get("CrystalMissile", pos);
			
			CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
			crystalMissile.owner              = this;
			crystalMissile.damage             = entityStat.attackDamage;
			crystalMissile.offset             = 0;
			crystalMissile.expectAttack       = false;
			crystalMissile.TimeBeforeLockOn   = timeBeforeLockOn;
			crystalMissile.LockOnDuration     = lockOnDuration;
			crystalMissile.initialSpeed       = speed;
			crystalMissile.transform.rotation = Quaternion.Euler(0, 0, rot);
			crystalMissile.Category           = "crystalMissile";
		}
		protected void ShootBigMissile(Vector2 pos, float rot, float speed = 13.5f, float timeBeforeLockOn = 0.01f, float lockOnDuration = 0f) {
			GameObject missile = UObjectPool.instance.Get("CrystalBigMissile", pos);
			
			CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
			crystalMissile.owner              = this;
			crystalMissile.damage             = entityStat.attackDamage;
			crystalMissile.offset             = 0;
			crystalMissile.expectAttack       = false;
			crystalMissile.TimeBeforeLockOn   = timeBeforeLockOn;
			crystalMissile.LockOnDuration     = lockOnDuration;
			crystalMissile.initialSpeed       = speed;
			crystalMissile.transform.rotation = Quaternion.Euler(0, 0, rot);
			crystalMissile.Category           = "crystalMissile";
		}
		
		// public virtual void WanderRoutine() {
		// 	if (!player) return;
		// 	// state           = EnemyState.Alert;
		// }
		
		public virtual void AlertRoutine() {
			if (!player) {
				// state = EnemyState.Wander;
				return;
			}
		
			if (attackable) {
				// state = EnemyState.Attack;
				return;
			}
		}
		
		public virtual void AttackReadyRoutine() { }
	}
}