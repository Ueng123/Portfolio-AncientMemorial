using UengSystem.Utility;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public abstract class CrystalPhase2Attack : CrystalPhase2State, IAttackState {
		private static readonly int ATTACKING          = "attacking".GetHash();
		
		public abstract float attackTime  { get; }
		public    float attackSpeed => crystal.stat.attackSpeed;
		protected int   step;
		
		public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		public bool  isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));

		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}
		
		public override void OnEnter() {
			step = 0;
			crystal.animator.SetBool(ATTACKING, true);
		}
		
		public override void OnEarlyRoutine() { }

		public override void OnExit() {
			stateMachine.AttackWatchTick();
			crystal.animator.SetBool(ATTACKING, false);
		}
	}
}
