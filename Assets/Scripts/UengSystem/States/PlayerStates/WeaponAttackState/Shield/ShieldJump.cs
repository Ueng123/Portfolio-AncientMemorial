using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace UengSystem.States.PlayerStates.WeaponAttackState.Shield {
	public class ShieldJump : PlayerWeaponAttack {
		public override float attackTime       => 0.2f;
		public override float attackAfterTime  => 5f;
		public override float attackSpeed => player.entityStat.attackSpeed / 3f;

		public override    void OnEnter() {
			base.OnEnter();
			
			if (player.entityStat.hp > player.entityData.hp * 0.5f) {
				player.SendAttackEvent(player, Mathf.Round(player.entityData.hp *0.2f), false);
			}
			
			player.SendEvent(EventType.Entity_Player_Jump, (int)EventPriority.Action);
			player.PlaySFX("shieldSkill");
			Vector2 footPosition = player.groundChecker.transform.position;
			UObjectPool.instance.Get("ShieldSkillEffect", footPosition);

			player.rigidbody2D.linearVelocityY = 2 + player.entityData.jumpPower*player.entityStat.moveSpeed*(player.entityData.hp - player.entityStat.hp)/
												 (player.entityData.hp*2.5f);
		}

		public override void OnEarlyRoutine() {
			if (step == 0 && isProgress(1)) {
				player.state = GetDefaultState();
			}
		}

		public override void OnRoutine() {
			float moveDir = InputManager.GetValue(ActionType.Move);
			Vector2 moveVec = new (moveDir * player.entityStat.moveSpeed, player.rigidbody2D.linearVelocity.y);
			
			if (moveDir != 0) player.rigidbody2D.AddForce(new Vector2(moveVec.x*50, 0)*Time.deltaTime, ForceMode2D.Force);
			
			float maxSpeed = player.entityStat.moveSpeed * 1.2f;
			player.rigidbody2D.linearVelocityX = Mathf.Clamp(player.rigidbody2D.linearVelocityX, -maxSpeed, maxSpeed);
		}
		
		public override void OnExit() { }
	}
}