using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Sword {
	public class Slash1 : PlayerWeaponAttack {

		// 정적 프로퍼티
		private static readonly int SLASH_EFFECT = "SlashEffect".GetHash();
		private static readonly int SWORD_SLASH_1 = "swordSlash1".GetHash();

		// 인스턴스 프로퍼티
		private float oldAnimatorSpeed;
		
		public override float attackTime       => 0.3f;
		public override float attackAfterTime  => 0.2f;
		public override float attackSpeed => player.stat.attackSpeed / 2f;

		// 오버라이드 메서드
		public override    void OnEnter() {
			base.OnEnter();
			player.animator.Play("Dattack2"+(player.lookingLeft?"B":""), 0, 0);
			
			oldAnimatorSpeed      = player.animator.speed;
			player.animator.speed = attackSpeed;
		}

		public override void OnRoutine() {
			if (step == 0 && isProgress(0.2f)) {
				Vector2 hitboxPos  = (Vector2)player.transform.position 
									 + new Vector2(1f*(player.lookingLeft?-1:1), -0.09f);
			
				Vector2 hitboxSize = new (1.5f, 0.3f);
				
				GameObject obj = UObject.Get(SLASH_EFFECT, (Vector2)player.transform.position + new Vector2(0.6f*(player.lookingLeft?-1:1)+Random.Range(-0.2f, 0.2f),-0.1f+Random.Range(-0.2f, 0.2f)), PlayEffect: false);
                obj.transform.rotation   = Quaternion.Euler(0, 0, 90 + Random.Range(-1f, 1f));
                obj.transform.localScale = new Vector3(1, 1.2f);
				
                float     randomDamage    = Random.Range(-0.1f, 0.1f);
                const int maxTargetEntity = 1;
                Entity.AttackAreaNoEffect(player, 2f+randomDamage, 0, hitboxPos, hitboxSize, 0, maxTargetEntity);
                player.PlaySFX(SWORD_SLASH_1);

				step = 1;
			}
			
			if (step == 1 && isProgress(1)) {
				player.entityState = GetDefaultState();
			}
		}
		
		public override void OnExit() {
			player.animator.speed = oldAnimatorSpeed;
			player.animator.Play("idle", 0, 0);
		}
	}
}
