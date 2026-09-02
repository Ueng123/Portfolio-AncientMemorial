using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.Utility;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace UengSystem.States.PlayerStates {
	public class Dash : PlayerState {
		
		private static readonly int Moving   = Animator.StringToHash("moving");
		
		public const float dashTime = 0.2f;

		private StopWatch effectTime = new ();
		private const float dashEffectCount = 10f;
		private const float dashEffectTime = dashTime / dashEffectCount;

		private float oldGravityScale;
		
		private int dashSign;
		
		public override void OnEnter() {
			player.SendEvent(EventType.Entity_Player_Dash, EventPriority.Action);
			player.PlaySFX("playerDash");
			player.Invincible(dashTime+0.3f);
			
			player.animator.SetBool(Moving, true);
			
			oldGravityScale = player.rigidbody2D.gravityScale;
			float moveDir = InputManager.GetValue(ActionType.Move);
			int moveSign  = (int)Mathf.Sign(InputManager.GetValue(ActionType.Move));
			int mouseSign = player.lookingLeft ? -1 : 1;

			dashSign = moveDir == 0 ? mouseSign : moveSign;
			
			player.rigidbody2D.gravityScale = 0;
			
			effectTime.Tick();
		}

		public override void OnEarlyRoutine() {
			if (stateTimer.CheckIn(dashTime)) return;

			player.state = GetDefaultState();
		}

		public override void OnRoutine() {
			if (effectTime.CheckIn(dashEffectTime)) return;
			effectTime.Tick();
			
			player.SpawnAfterImage();
		}

		public override void OnFixedRoutine() {
			player.rigidbody2D.linearVelocity = Vector2.right * ((10 + player.stat.moveSpeed * 0.75f)* dashSign);
		}

		public override    void OnExit() {
			player.animator.SetBool(Moving, false);
			
			player.rigidbody2D.gravityScale   = oldGravityScale;
			player.rigidbody2D.linearVelocity = Vector2.right * (player.stat.moveSpeed * dashSign);
		}
	}
}
