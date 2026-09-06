using UengSystem.Events;
using UengSystem.Inputs;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.States.PlayerStates {
	public class Move : PlayerDefaultState {
		private static readonly int Moving   = Animator.StringToHash("moving");

		private float  moveDir;
		
		private bool _FootstepSound = false;
		private static string[] footstepNames = { "footstep1", "footstep2", "footstep3" };
		private bool FootstepSound {
			get => _FootstepSound;
			set {
				if (_FootstepSound == value) return;
				if (value) player.PlaySFX(footstepNames[Random.Range(0, footstepNames.Length)]);
				_FootstepSound = value;
			}
		}
		
		public override    void OnEnter() {
			player.SendEvent(EventType.Entity_Player_MoveStart, EventPriority.Start);
			
			moveDir = InputManager.GetValue(ActionType.Move);
			player.animator.SetBool(Moving, true);
		}

		public override void OnEarlyRoutine() {
			base.OnEarlyRoutine();
			
			moveDir = InputManager.GetValue(ActionType.Move);
		}

		public override void OnRoutine() {
			FootstepSound = player.spriteRenderer.sprite.name.Equals("Player_2"); 
			
			float step = 6 * player.stat.moveSpeed * Time.deltaTime;
			
			int velocitySign = player.rigidbody2D.linearVelocityX == 0
								   ? (int)moveDir
								   : (int)Mathf.Sign(player.rigidbody2D.linearVelocityX);

			
			// 이동 반대 방향으로의 속도에는 급격한 감속 적용 
			int dirMult = moveDir * velocitySign > 0 ? 1 : -2;
			
			// 이동중이 아닐때 감속
			int stepMulitplier = velocitySign * (moveDir == 0 ? -1 : dirMult);
			
			float newVelocityX = Mathf.Clamp(player.rigidbody2D.linearVelocityX + step * stepMulitplier,
											 -player.stat.moveSpeed,
											 player.stat.moveSpeed);

			bool isVelocityAlmostZero = newVelocityX is >= -0.05f and <= 0.05f;
			player.rigidbody2D.linearVelocityX = moveDir == 0 && isVelocityAlmostZero ? 0 : newVelocityX;
		}

		public override void OnExit() {
			player.SendEvent(EventType.Entity_Player_MoveStop, EventPriority.Stop);
			
			player.animator.SetBool(Moving, false);
		}
	}
}
