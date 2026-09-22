using UengSystem.Utility;
using UengSystem.Objects;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Shield {
	public class ShieldJump : PlayerWeaponAttack {

		// 정적 프로퍼티
		private static readonly int SHIELD_SKILL_EFFECT = "ShieldSkillEffect".GetHash();
		private static readonly int SHIELD_SKILL = "shieldSkill".GetHash();

		// 인스턴스 프로퍼티
		public override float attackTime       => 0.2f;
		public override float attackAfterTime  => 5f;
		public override float attackSpeed => player.stat.attackSpeed / 3f;

		// 오버라이드 메서드
		public override    void OnEnter() {
			base.OnEnter();
			
			if (player.stat.HP > player.data.HP * 0.5f) {
				player.SendAttackEvent(player, Mathf.Round(player.data.HP *0.2f), false);
			}
			
			player.SendEvent(EventType.Entity_Player_Jump, EventPriority.Action);
			player.PlaySFX(SHIELD_SKILL);
			Vector2 footPosition = player.groundChecker.transform.position;
			UObject.Get(SHIELD_SKILL_EFFECT, footPosition, PlayEffect: false);

			player.rigidbody2D.linearVelocityY = 2 + player.data.jumpPower*player.stat.moveSpeed*(player.data.HP - player.stat.HP)/
												 (player.data.HP*2.5f);
		}

		public override void OnEarlyRoutine() {
			if (step == 0 && isProgress(1)) {
				player.entityState = GetDefaultState();
			}
		}

		public override void OnRoutine() {
			float moveDir = InputManager.GetValue(ActionType.Move);
			Vector2 moveVec = new (moveDir * player.stat.moveSpeed, player.rigidbody2D.linearVelocity.y);
			
			if (moveDir != 0) player.rigidbody2D.AddForce(new Vector2(moveVec.x*50, 0)*Time.deltaTime, ForceMode2D.Force);
			
			float maxSpeed = player.stat.moveSpeed * 1.2f;
			player.rigidbody2D.linearVelocityX = Mathf.Clamp(player.rigidbody2D.linearVelocityX, -maxSpeed, maxSpeed);
		}
		
		public override void OnExit() { }
	}
}
