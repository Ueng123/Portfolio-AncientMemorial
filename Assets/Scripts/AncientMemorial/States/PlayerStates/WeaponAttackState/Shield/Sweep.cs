using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.States.PlayerStates.WeaponAttackState.Shield {
	public class Sweep : PlayerWeaponAttack {
		private float oldAnimatorSpeed;
		
		public override float attackTime       => 0.5f;
		public override float attackAfterTime  => 0.5f;
		public override float attackSpeed => player.stat.attackSpeed / 3f;

		public override    void OnEnter() {
			base.OnEnter();
			
			player.rigidbody2D.linearVelocityX = 0;
			
			oldAnimatorSpeed      = player.animator.speed;
			player.animator.speed = attackSpeed;
			
			player.animator.Play("Sattack1"+(player.lookingLeft?"B":""), 0);
		}

		public override void OnRoutine() {
			if (step == 0 && isProgress(0.2f)) {
				Vector2 hitboxPos  = (Vector2)player.transform.position 
									 + new Vector2(0.75f*(player.lookingLeft?-1:1), 0f);
				
				Vector2 hitboxSize = new (1.5f,                            0.6f);
				
				float     randomDamage    = Random.Range(-0.1f, 0.1f);
				const int maxTargetEntity = 1;
				Entity.AttackAreaNoEffect(player, 1+randomDamage, 0, hitboxPos, hitboxSize, 0, maxTargetEntity);
				player.PlaySFX("shieldSweep", pitch:1.5f);

				step = 1;
			}

			if (step == 1 && isProgress(1)) {
				player.state = GetDefaultState();
			} 
		}

		public override    void OnExit() {
			player.animator.speed = oldAnimatorSpeed;
			player.animator.Play("idle");
		}
	}
}