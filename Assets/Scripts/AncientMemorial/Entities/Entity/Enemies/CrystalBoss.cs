using AncientMemorial.Cameras;
using AncientMemorial.Projectiles;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.States.EnemyStates;
using UengSystem.States.EnemyStates.Crystal;
using UengSystem.UAction;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public abstract class CrystalBoss : Enemy {
		public GameObject[] hideOnDeath;
		
		protected override void        OnGrounded() { }

		protected void InitializeCrystalStateMachine(CrystalState idleState) {
			InitializeStateMachine(idleState, idleState, new EmptyAttackReady());
		}
		
		public override void OnHit(Entity        attacker, float damage, Vector2? pushDir = null) {
			bool groggy = (state == stateMachine.stunState);
			
			if (groggy) { ShowCriticalDamageUI(damage); }
			else { ShowDamageUI(damage); }
			
			if (stat.HP > 0) {
				PlaySFX("crystalHit1");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0, 0.05f);
				CameraBrain.instance.ShakeLerp(1, 10);
				CameraBrain.instance.ZoomLerp(-0.1f, 20f);
			}
		}
		
		protected override float GetRealDamage(float rawDamage) {
			bool groggy = (state == stateMachine.stunState);
			return rawDamage * (groggy ? 2 : 1);
		}

		public override void Initialize() {
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(true);
			}
			
			base.Initialize();
		}

		protected override void Death() {
			GameManager.SetTimeScale(0.5f, 0.25f);
			CameraBrain.instance.ShakeLerp(5, 1);
			CameraBrain.instance.ZoomLerp(-1f, 7.5f);

			PlaySFX("crystalHit2");
			
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(false);
			}
			
			for (int i = 0; i < 10; i++) {
				UObjectPool.instance.Get("crystalDebris", (Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
			}
			
			base.Death();
		}
	}
}
