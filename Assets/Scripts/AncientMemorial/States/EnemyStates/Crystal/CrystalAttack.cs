namespace AncientMemorial.States.EnemyStates.Crystal {
	public abstract class CrystalAttack : CrystalState, IAttackState {

		// 인스턴스 프로퍼티
		public abstract float attackTime  { get; }
		public          float attackSpeed => crystal.stat.attackSpeed;
		protected       int   step;

		// 인스턴스 메서드
		public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		public bool  isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));

		// 오버라이드 메서드
		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}
		
		public override void OnEnter() {
			step = 0;
		}
		
		public override void OnEarlyRoutine() { }

		public override void OnExit() { }
	}
}
