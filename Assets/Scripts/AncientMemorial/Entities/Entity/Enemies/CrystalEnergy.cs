using System;
using System.Collections;
using AncientMemorial.Cameras;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Objects.LifeCycle;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities.Enemies {
	public class CrystalEnergy : Enemy {

		// 인스턴스 프로퍼티
		public Animator rotatingAnimator;
		private float lastHitDamage;

		// 오버라이드 메서드
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
				CameraManager.instance.ShakeLerp(1f, 5);
				CameraManager.instance.ZoomLerp(-0.1f);
			}
		}

		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}

		protected override Entity GetTargetEntity() => null;

		public override void Attack() { }
		
		public override void OnFirstGet() {
			SetDefaultStates(ReleasingState: new AnimationReleasing(this, "dead"));
			base.OnFirstGet();
		}

		protected override void Death() {
			GameManager.SetTimeScale(0, 0.15f);
			CameraManager.instance.ShakeLerp(3f * Mathf.Max(Mathf.Log(lastHitDamage + 3), 0.5f), 3);
			CameraManager.instance.ZoomLerp(-2f);
			
			base.Death();
		}
		
	}
}
