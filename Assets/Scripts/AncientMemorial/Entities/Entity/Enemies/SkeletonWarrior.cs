using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Skeleton;
using AncientMemorial.States.EnemyStates.Skeleton.Warrior;

namespace AncientMemorial.Entities.Enemies {
	public class SkeletonWarrior : Skeleton {
		private EnemyState pierce;
		
		public override void OnFirstGet() {
			base.OnFirstGet();
			
			EnemyState wanderState      = new SkeletonWander(2, 5);
			EnemyState awareState       = new SkeletonAware(3, 1, 0.75f, -0.75f, 3);
			EnemyState attackReadyState = new SkeletonAttackReady(0f, 1f, 0.1f, -0.1f, 1);

			InitializeStateMachine(wanderState, awareState, attackReadyState);

			pierce = new SkeletonWarriorPierce().Init(stateMachine);
		}

		protected override Entity GetTargetEntity() => player;

		public override void Attack() {
			entityState = pierce;
		}
	}
}