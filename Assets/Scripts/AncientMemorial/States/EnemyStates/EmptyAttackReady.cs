namespace AncientMemorial.States.EnemyStates {
	public class EmptyAttackReady : EnemyState {

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			stateMachine.Attack();
		}
	}
}