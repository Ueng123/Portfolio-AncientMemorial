using AncientMemorial.Entities;

namespace UengSystem.States.EnemyStates.Skeleton {
	public abstract class SkeletonAttack : SkeletonState, IAttackState {
		protected int step;

		public abstract float attackTime  { get; }
		public          float attackSpeed => enemy.entityStat.attackSpeed;

		public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		public bool isProgress(float percent) => !stateTimer.Check(GetDelay(percent));

		public override void OnEnter() {
			step = 0;
		}

		public override void OnEarlyRoutine() { }

		public override void OnExit() {
			stateMachine.AttackWatchTick();
		}
	}
}