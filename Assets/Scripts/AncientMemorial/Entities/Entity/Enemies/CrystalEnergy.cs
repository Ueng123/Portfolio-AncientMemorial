using System;
using System.Collections;
using AncientMemorial.Cameras;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.States.EnemyStates.Crystal.Phase3;
using UengSystem.UAction;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities.Enemies {
	public class CrystalEnergy : Enemy {
		public Animator rotatingAnimator;
		private float lastHitDamage;

		protected override void OnGrounded() { }

		public override void Initialize() {
			lastHitDamage = 0f;
			base.Initialize();
			animator.Play("spawn");
		}

		protected override void Routine() {
			base.Routine();
			rotatingAnimator.speed = Mathf.Lerp(rotatingAnimator.speed, 1, Time.deltaTime);
		}

		public override    void        OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			lastHitDamage = damage;
			rotatingAnimator.speed = 15;
			
			ShowCriticalDamageUI(damage);
			
			if (stat.HP > 0) {
				GameManager.SetTimeScale(0, 0.05f);
				CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.2f);
			}
		}

		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}

		protected override Entity GetTargetEntity() => null;

		public override void Attack() { }
		
		protected override void PrepareDespawnFX() {
			ToggleColliders(false);
		}

		protected override void Death() {
			GameManager.SetTimeScale(0, 0.15f);
			CameraBrain.instance.ShakeLerp(3f * Mathf.Max(Mathf.Log(lastHitDamage + 3), 0.5f), 3);
			CameraBrain.instance.ZoomLerp(-2f);
			
			base.Death();
		}
		
		protected override IEnumerator DespawnFX(float duration) {
			animator.Play("dead");
			yield return new WaitForSeconds(duration);
			UObjectPool.instance.Release(gameObject, -1);
		}
	}
}
