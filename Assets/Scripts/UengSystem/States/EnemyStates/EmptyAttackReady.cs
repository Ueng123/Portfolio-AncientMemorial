namespace UengSystem.States.EnemyStates.Skeleton {
	public class EmptyAttackReady : EnemyState {
		public override void OnEnter() {
			base.OnEnter();
			stateMachine.Attack();
		}
	}
}