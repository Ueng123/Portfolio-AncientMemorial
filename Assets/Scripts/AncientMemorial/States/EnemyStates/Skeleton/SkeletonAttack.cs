using AncientMemorial.Entities;

namespace UengSystem.States.EnemyStates.Skeleton {
	public abstract class SkeletonAttack : SkeletonState, IAttackState {
		protected int step;

		public abstract float attackTime  { get; }
		public          float attackSpeed => enemy.stat.attackSpeed;

		public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		public bool isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));

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
