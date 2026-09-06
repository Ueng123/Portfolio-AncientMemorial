using UengSystem.Objects;
using AncientMemorial.Cameras;
using UengSystem;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public class Scarecrow : Enemy {
		protected override Entity GetTargetEntity() => player;
		
		public override void Attack() { }

		public override bool isAttackTarget(Entity entity) => false;

		protected override void OnGrounded() { }
		
		protected override void OpenEntityUI() { }

		public override    void  OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			ShowDamageUI(damage);
			
			PlaySFX("scarecrowHit");
			animator.Play("Hit");
			
			if (attacker != player) return;
			GameManager.SetTimeScale(0f, 0.05f);
			CameraBrain.instance.ShakeLerp(1, 5);
			CameraBrain.instance.ZoomLerp(-0.1f);
		}

		protected override void Death() {
			UObject.Get("Explode2", transform.position, PlayEffect: false);
			base.Death();
		}

		public override void ChangeHP(float amount) {
			base.ChangeHP(amount/1000f);
		}

		protected override float GetRealDamage(float rawDamage) => rawDamage;
	}
}
