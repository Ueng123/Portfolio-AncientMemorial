namespace AncientMemorial.States.EnemyStates.Skeleton {
	public abstract class SkeletonAttack : SkeletonState, IAttackState {

		// 인스턴스 프로퍼티
		protected int step;

		public abstract float attackTime  { get; }
		public          float attackSpeed => enemy.stat.attackSpeed;

		// 인스턴스 메서드
		public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		public bool isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));

		// 오버라이드 메서드
		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}
		
		public override void OnEnter() {
			step = 0;
		}

		public override void OnEarlyRoutine() { }

		public override void OnExit() {
			stateMachine.AttackWatchTick();
		}
	}
}
