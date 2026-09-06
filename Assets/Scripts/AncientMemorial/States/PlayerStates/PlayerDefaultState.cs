namespace AncientMemorial.States.PlayerStates {
	public abstract class PlayerDefaultState : PlayerState {
		public override void OnEarlyRoutine() {
			if (player.entityState == GetDefaultState()) return;
			player.entityState = GetDefaultState();
		}
	}
}