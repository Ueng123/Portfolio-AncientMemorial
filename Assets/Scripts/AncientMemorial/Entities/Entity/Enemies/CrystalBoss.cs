using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Cameras;
using AncientMemorial.Projectiles;
using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Crystal;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.UAction;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public abstract class CrystalBoss : Enemy {

		// 정적 프로퍼티
		private static readonly int CrystalDebrisPrefabId = "crystalDebris".GetHash();
		private static readonly int CrystalHit1ClipId = "crystalHit1".GetHash();
		private static readonly int CrystalHit2ClipId = "crystalHit2".GetHash();

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
				PlaySFX(CrystalHit1ClipId);
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0, 0.05f);
				CameraBrain.instance.ShakeLerp(1, 10);
				CameraBrain.instance.ZoomLerp(-0.1f, 20f);
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
			CameraBrain.instance.ShakeLerp(5, 1);
			CameraBrain.instance.ZoomLerp(-1f, 7.5f);

			PlaySFX(CrystalHit2ClipId);
			
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(false);
			}
			
			for (int i = 0; i < 10; i++) {
				UObject.Get(CrystalDebrisPrefabId, (Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), PlayEffect: false);
			}
			
			base.Death();
		}
	}
}
