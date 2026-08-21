using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.States.PlayerStates.WeaponAttackState.Shield {
	public class Strike : PlayerWeaponAttack {

		public float oldAnimatorSpeed;
		
		public override float attackTime       => 0.5f;
		public override float attackAfterTime  => 0.75f;
		public override float attackSpeed => player.entityStat.attackSpeed / 3f;

		public override void OnEnter() {
			base.OnEnter();
			
			player.rigidbody2D.linearVelocityX = 0;
			
			oldAnimatorSpeed      = player.animator.speed;
			player.animator.speed = attackSpeed;
			
			player.animator.Play(player.lookingLeft?"Sattack2B":"Sattack2", 0, 0);
		}

		public override void OnRoutine() {
			if (step == 0 && isProgress(0.6f)) {
				Vector2 hitboxPos  = (Vector2)player.transform.position
									 +new Vector2(0.75f*(player.lookingLeft?-1:1), -0.3f);
				
				Vector2 hitboxSize = new (1.75f, 0.4f);
				
				UObjectPool.instance.Get("Explode4", (Vector2)player.transform.position + new Vector2(0.35f*(player.lookingLeft?-1:1)+Random.Range(-0.05f, 0.05f),-0.4f+Random.Range(-0.05f, 0.05f)));
			
				// damage
				float     damage          = Mathf.Pow(player.entityData.hp / 10f, 1.5f) / player.entityStat.attackDamage;
				const int maxTargetEntity = 2;
				Entity.AttackAreaNoEffect(player, damage, 0, hitboxPos, hitboxSize, 0, maxTargetEntity);
				player.PlaySFX("shieldSweep");

				step = 1;
			}

			if (step == 1 && isProgress(1)) {
				player.state = GetDefaultState();
			}
		}
		
		public override void OnExit() {
			player.animator.speed = oldAnimatorSpeed;
			player.animator.Play("idle");
		}
	}
}