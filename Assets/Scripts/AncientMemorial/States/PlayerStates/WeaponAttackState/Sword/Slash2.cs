using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Sword {
	public class Slash2 : PlayerWeaponAttack {

		// 정적 프로퍼티
		private static readonly int SlashEffectPrefabId = "SlashEffect".GetHash();
		private static readonly int SwordSlash2ClipId = "swordSlash2".GetHash();

		// 인스턴스 프로퍼티
		private float oldAnimatorSpeed;
		
		public override float attackTime       => 0.3f;
		public override float attackAfterTime  => 0.2f;
		public override float attackSpeed => player.stat.attackSpeed / 2f;

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			player.animator.Play("Dattack3"+(player.lookingLeft?"B":""), 0, 0);
			
			oldAnimatorSpeed      = player.animator.speed;
			player.animator.speed = attackSpeed;
		}
		
		public override void OnRoutine() {
			if (step == 0 && isProgress(0.2f)) {
				Vector2 hitboxPos  = (Vector2)player.transform.position
									 + new Vector2(0.3f*(player.lookingLeft?-1:1), -0.2f);
				
				Vector2 hitboxSize = new (1.7f, 0.4f);
			
				GameObject obj = UObject.Get(SlashEffectPrefabId, (Vector2)player.transform.position + new Vector2(0.3f*(player.lookingLeft?-1:1)+Random.Range(-0.1f, 0.1f),-0.1f+Random.Range(-0.1f, 0.1f)), PlayEffect: false);
				obj.transform.rotation   = Quaternion.Euler(0, 0, -85 + Random.Range(-2f, 2f));
				obj.transform.localScale = new Vector3(1.1f, 2f);
				
				float     randomDamage    = Random.Range(-0.2f, 0.2f);
				const int maxTargetEntity = 1;
				Entity.AttackAreaNoEffect(player, 2.7f+randomDamage, 0, hitboxPos, hitboxSize, 0, maxTargetEntity);
				player.PlaySFX(SwordSlash2ClipId);

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
