namespace UengSystem.States.PlayerStates {
	public abstract class PlayerDefaultState : PlayerState {
		public override void OnEarlyRoutine() {
			if (player.state == GetDefaultState()) return;
			player.state = GetDefaultState();
		}
	}
}