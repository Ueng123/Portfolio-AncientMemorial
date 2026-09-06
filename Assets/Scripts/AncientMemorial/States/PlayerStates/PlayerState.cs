using AncientMemorial.Entities;
using UengSystem.Inputs;
using UengSystem.States;

namespace AncientMemorial.States.PlayerStates {
	public abstract class PlayerState : UState {
		
		protected Player player => Entity.player;

		protected static PlayerDefaultState GetDefaultState() {
			if (!Entity.player.isGround) {
				return fall;
			}
			
			float moveDir = InputManager.GetValue(ActionType.Move);
			bool  moving  = moveDir != 0 || Entity.player.rigidbody2D.linearVelocityX != 0;
			
			return moving ? move : idle;
		}
		
		public static Idle idle = new Idle();
		public static Move move = new Move();
		public static Fall fall = new Fall();
		
		public static Jump jump = new Jump();
		public static Dash dash = new Dash();

		public override void OnEarlyRoutine() { }
		
		public override void OnRoutine() { }

		public override void OnLateRoutine() { }

		public override void OnFixedRoutine() { }
	}
}