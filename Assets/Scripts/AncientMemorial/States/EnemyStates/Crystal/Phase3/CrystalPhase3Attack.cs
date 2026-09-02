namespace UengSystem.States.EnemyStates.Crystal.Phase3 {
	public abstract class CrystalPhase3Attack : CrystalPhase3State{
		public abstract float attackTime  { get; }
		public          float attackSpeed => crystal.stat.attackSpeed;
		protected       int   step;
		
		public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		public bool  isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));

		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}
		
		public override void OnEnter() {
			step = 0;
		}
		
		public override void OnEarlyRoutine() { }
	}
}
