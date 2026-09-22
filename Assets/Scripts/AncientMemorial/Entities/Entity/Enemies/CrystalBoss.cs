using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Cameras;
using AncientMemorial.Projectiles;
using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Crystal;
using UengSystem;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public abstract class CrystalBoss : Enemy {

		// 정적 프로퍼티
		private static readonly int CRYSTAL_DEBRIS = "crystalDebris".GetHash();
		private static readonly int CRYSTAL_HIT_1 = "crystalHit1".GetHash();
		private static readonly int CRYSTAL_HIT_2 = "crystalHit2".GetHash();

		// 인스턴스 프로퍼티
		public GameObject[] hideOnDeath;

		// 인스턴스 메서드
		protected void InitializeCrystalStateMachine(CrystalState idleState) {
			InitializeStateMachine(idleState, idleState, new EmptyAttackReady());
		}

		// 오버라이드 메서드
		protected override void        OnGrounded() { }
		
		public override void OnHit(Entity        attacker, float damage, Vector2? pushDir = null) {
			bool groggy = (entityState == stateMachine.stunState);
			
			if (groggy) { ShowCriticalDamageUI(damage); }
			else { ShowDamageUI(damage); }
			
			if (stat.HP > 0) {
				PlaySFX(CRYSTAL_HIT_1);
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0, 0.05f);
				CameraManager.instance.ShakeLerp(1, 10);
				CameraManager.instance.ZoomLerp(-0.1f, 20f);
			}
		}
		
		protected override float GetRealDamage(float rawDamage) {
			bool groggy = (entityState == stateMachine.stunState);
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
			CameraManager.instance.ShakeLerp(5, 1);
			CameraManager.instance.ZoomLerp(-1f, 7.5f);

			PlaySFX(CRYSTAL_HIT_2);
			
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(false);
			}
			
			for (int i = 0; i < 10; i++) {
				UObject.Get(CRYSTAL_DEBRIS, (Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), PlayEffect: false);
			}
			
			base.Death();
		}
	}
}
