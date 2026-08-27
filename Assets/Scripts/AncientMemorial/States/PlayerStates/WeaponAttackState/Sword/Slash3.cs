using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.States.PlayerStates.WeaponAttackState.Sword {
	public class Slash3 : PlayerWeaponAttack {
		private float oldAnimatorSpeed;
		
		public override float attackTime       => 0.3f;
		public override float attackAfterTime  => 0.4f;
		public override float attackSpeed => player.entityStat.attackSpeed / 2f;

		public override void OnEnter() {
			base.OnEnter();
			player.animator.Play("Dattack1"+(player.lookingLeft?"B":""), 0, 0);
			
			oldAnimatorSpeed      = player.animator.speed;
			player.animator.speed = attackSpeed;
		}

		public override void OnRoutine() {
			if (step == 0 && isProgress(0.2f)) {
				Vector2 hitboxPos  = (Vector2)player.transform.position
									 + new Vector2(0.6f*(player.lookingLeft?-1:1), -0.1f);
			
				Vector2 hitboxSize = new (1f, 0.7f);
			
				GameObject obj = UObjectPool.instance.Get("SlashEffect", (Vector2)player.transform.position + new Vector2(0.48f*(player.lookingLeft?-1:1)+Random.Range(-0.1f, 0.1f),Random.Range(-0.1f, 0.1f)));
				obj.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
				obj.transform.localScale = new Vector3(0.9f, 1.2f);
			
				float     randomDamage    = Random.Range(-0.3f, 0.3f);
				const int maxTargetEntity = 3;
				Entity.AttackAreaNoEffect(player, 3f+randomDamage, 0, hitboxPos, hitboxSize, 0, maxTargetEntity);
				player.PlaySFX("swordSlash3");

				step = 1;
			}
			
			if (step == 1 && isProgress(1)) {
				player.state = GetDefaultState();
			}
		}
		
		public override void OnExit() {
			player.animator.speed = oldAnimatorSpeed;
			player.animator.Play("idle", 0, 0);
		}
	}
}