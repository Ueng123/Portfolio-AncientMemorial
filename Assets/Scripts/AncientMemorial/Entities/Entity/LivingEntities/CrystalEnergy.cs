using System.Collections;
using AncientMemorial.Cameras;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class CrystalEnergy : Enemy {
		private static readonly int  Dead = Animator.StringToHash("dead");
		protected override      void OnGrounded() { }

		public Animator rotatingAnimator;
		
		public override    void        OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			rotatingAnimator.speed = 15;

			PlaySFX("crystalRoar", volume:0.1f, pitch: 3f);
			ShowCriticalDamageUI(damage);
			
			if (entityStat.hp <= 0) {
				PlaySFX("crystalHit2");
				GameManager.SetTimeScale(0, 0.15f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-2f);
				return;
			}
			
			PlaySFX("crystalHit1");
			if (attacker != player) return;
			GameManager.SetTimeScale(0, 0.05f);
			CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-0.2f);
		}

		protected override float GetRealDamage(float rawDamage) { return rawDamage; }

		public override void OnStunStart() { }

		public override void OnStunEnd() { }

		protected override IEnumerator AttackEnumerator() {
			yield return new WaitForEndOfFrame();
		}

		protected override void OnAttackDone() { }

		protected override void OnAttackCancel() { }

		// public override void WanderRoutine() {
		// 	rotatingAnimator.speed = Mathf.Lerp(rotatingAnimator.speed, 1, Time.deltaTime);
		// }
		
		public override void Initialize() {
			base.Initialize();
			animator.Play("spawn");
		}

		protected override void PrepareDespawnFX() {
			ToggleColliders(false);
		}
		
		protected override IEnumerator DespawnFX(float duration) {
			animator.Play("dead");
			yield return CacheManager.WaitForSeconds(duration);
			UObjectPool.instance.Release(gameObject, -1);
		}

		protected override void Death() {
			GameManager.UValueFloatVariables["crystalEnergyDead"] = new UPureFloat {number = GameManager.UValueFloatVariables["crystalEnergyDead"].value + 1};
			GameManager.UValueFloatVariables["crystalEnergyGimmick"] = new UPureFloat {number = GameManager.UValueFloatVariables["crystalEnergyGimmick"].value - 1};
			
			PlaySFX("crystalRoar", pitch: 0.75f);
			
			base.Death();
		}
	}
}