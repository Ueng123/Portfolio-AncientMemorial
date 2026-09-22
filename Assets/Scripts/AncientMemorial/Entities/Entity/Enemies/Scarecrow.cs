using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Cameras;
using UengSystem;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public class Scarecrow : Enemy {

		// 정적 프로퍼티
		private static readonly int EXPLODE_2 = "Explode2".GetHash();
		private static readonly int SCARECROW_HIT = "scarecrowHit".GetHash();

		// 오버라이드 메서드
		protected override Entity GetTargetEntity() => player;
		
		public override void Attack() { }

		public override bool isAttackTarget(Entity entity) => false;

		protected override void OnGrounded() { }
		
		protected override void OpenEntityUI() { }

		public override    void  OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			ShowDamageUI(damage);
			
			PlaySFX(SCARECROW_HIT);
			animator.Play("Hit");
			
			if (attacker != player) return;
			GameManager.SetTimeScale(0f, 0.05f);
			CameraManager.instance.ShakeLerp(1, 5);
			CameraManager.instance.ZoomLerp(-0.1f);
		}

		protected override void Death() {
			UObject.Get(EXPLODE_2, transform.position, PlayEffect: false);
			base.Death();
		}

		public override void ChangeHP(float amount) {
			base.ChangeHP(amount/1000f);
		}

		protected override float GetRealDamage(float rawDamage) => rawDamage;
	}
}
