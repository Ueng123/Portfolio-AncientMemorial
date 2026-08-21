using System.Collections;
using AncientMemorial.Cameras;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.ObjectPool;
using UengSystem.States.EnemyStates;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class Scarecrow : Enemy {

		public override Entity GetTargetEntity() => player;
		
		public override void Attack() { }

		public override bool isAttackTarget(Entity entity) => false;

		protected override void OnGrounded() { }
		
		protected override void OpenEntityUI() { }

		public override    void  OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			ShowDamageUI(damage);
			
			PlaySFX("scarecrowHit");
			animator.Play("Hit");
			
			if (attacker != player) return;
			if (entityStat.hp <= 0) {
				UObjectPool.instance.Get("Explode2", transform.position);
				GameManager.UValueFloatVariables["scarecrowDead"]   = new UPureFloat {number = GameManager.UValueFloatVariables["scarecrowDead"].value + 1};
			}
			
			GameManager.SetTimeScale(0f, 0.05f);
			CameraBrain.instance.ShakeLerp(1, 5);
			CameraBrain.instance.ZoomLerp(-0.1f);
		}

		public override void ChangeHP(float amount) {
			base.ChangeHP(amount/1000f);
		}

		protected override float GetRealDamage(float rawDamage) => rawDamage;
	}
}