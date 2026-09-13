using UengSystem.Inputs;
using UnityEngine;

namespace AncientMemorial.States.PlayerStates {
	public class Fall : PlayerDefaultState {

		// 오버라이드 메서드
		public override void OnEnter() { }

		public override void OnRoutine() {
			float   moveDir = InputManager.GetValue(ActionType.Move);
			Vector2 moveVec = new (moveDir * player.stat.moveSpeed, player.rigidbody2D.linearVelocity.y);
			
			if (moveDir == 0) return;
			
			player.rigidbody2D.AddForce(new Vector2(moveVec.x*50, 0)*Time.deltaTime, ForceMode2D.Force);

			float maxSpeed = player.stat.moveSpeed * 1.2f;
			player.rigidbody2D.linearVelocityX = Mathf.Clamp(player.rigidbody2D.linearVelocityX, -maxSpeed, maxSpeed);
		}

		public override void OnExit() { }
	}
}