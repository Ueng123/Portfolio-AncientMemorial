using UengSystem.Events;
using UengSystem.Inputs;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace UengSystem.States.PlayerStates {
	public class Jump : PlayerState {
		private const float   jumpLengthMin = 0.1f;
		private const float   jumpLengthMax = 0.75f;
		
		public override    void OnEnter() {
			player.SendEvent(EventType.Entity_Behaviour_Jump, (int)EventPriority.Action);
			player.PlaySFX("playerJump");

			player.rigidbody2D.linearVelocityY = player.entityStat.jumpPower;
		}

		public override void OnEarlyRoutine() {
			bool isTimeLess =  stateTimer.Check(jumpLengthMin);
			bool isTimeOver = !stateTimer.Check(jumpLengthMax);
			bool keyInput   = InputManager.GetInput(ActionType.Jump, PressType.Up|PressType.None);
				
			bool jumpLoopEnd = isTimeOver || keyInput || player.isGround;

			if (jumpLoopEnd && !isTimeLess) {
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
		
		public override void OnExit() {
			player.rigidbody2D.linearVelocityY = Mathf.Min(player.rigidbody2D.linearVelocityY-0.75f, 2);
		}
	}
}