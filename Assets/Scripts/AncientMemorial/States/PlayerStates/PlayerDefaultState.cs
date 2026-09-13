namespace AncientMemorial.States.PlayerStates {
	public abstract class PlayerDefaultState : PlayerState {

		// 오버라이드 메서드
		public override void OnEarlyRoutine() {
			if (player.entityState == GetDefaultState()) return;
			player.entityState = GetDefaultState();
		}
	}
}