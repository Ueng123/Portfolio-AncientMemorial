namespace AncientMemorial.States.EnemyStates {
	public class EmptyAttackReady : EnemyState {
		public override void OnEnter() {
			base.OnEnter();
			stateMachine.Attack();
		}
	}
}