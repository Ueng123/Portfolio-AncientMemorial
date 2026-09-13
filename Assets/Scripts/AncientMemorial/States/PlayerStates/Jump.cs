using UengSystem.Utility;
using UengSystem.Events;
using UengSystem.Inputs;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.States.PlayerStates {
	public class Jump : PlayerState {

		// 정적 프로퍼티
		private static readonly int PlayerJumpClipId = "playerJump".GetHash();

		private const float   jumpLengthMin = 0.1f;
		private const float   jumpLengthMax = 0.75f;

		// 오버라이드 메서드
		public override    void OnEnter() {
			player.SendEvent(EventType.Entity_Player_Jump, EventPriority.Action);
			player.PlaySFX(PlayerJumpClipId);

			player.rigidbody2D.linearVelocityY = player.stat.jumpPower;
		}

		public override void OnEarlyRoutine() {
			bool isTimeLess =  stateTimer.CheckIn(jumpLengthMin);
			bool isTimeOver = stateTimer.CheckOut(jumpLengthMax);
			bool keyInput   = InputManager.GetInput(ActionType.Jump, InputState.Up|InputState.None);
				
			bool jumpLoopEnd = isTimeOver || keyInput || player.isGround;

			if (jumpLoopEnd && !isTimeLess) {
				player.entityState = GetDefaultState();
			}
		}

		public override void OnFixedRoutine() {
			float   moveDir = InputManager.GetValue(ActionType.Move);
			Vector2 moveVec = new (moveDir * player.stat.moveSpeed, player.rigidbody2D.linearVelocity.y);
			
			if (moveDir != 0) player.rigidbody2D.AddForce(new Vector2(moveVec.x*50, 0)*Time.deltaTime, ForceMode2D.Force);
			
			float maxSpeed = player.stat.moveSpeed * 1.2f;
			player.rigidbody2D.linearVelocityX = Mathf.Clamp(player.rigidbody2D.linearVelocityX, -maxSpeed, maxSpeed);
		}
		
		public override void OnExit() {
			player.rigidbody2D.linearVelocityY = Mathf.Min(player.rigidbody2D.linearVelocityY-0.75f, 2);
		}
	}
}
