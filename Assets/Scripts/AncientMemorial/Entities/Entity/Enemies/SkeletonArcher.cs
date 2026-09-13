using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Skeleton;
using AncientMemorial.States.EnemyStates.Skeleton.Archer;

namespace AncientMemorial.Entities.Enemies {
	public class SkeletonArcher : Skeleton {

		// 인스턴스 프로퍼티
		private EnemyState shoot;

		// 오버라이드 메서드
		public override void OnFirstGet() {
			base.OnFirstGet();
			
			EnemyState wanderState      = new SkeletonWander(3, 5);
			EnemyState awareState       = new SkeletonAware(6, 1, 0.5f, -1f, 3);
			EnemyState attackReadyState = new SkeletonAttackReady(6, 1, 0.5f, -1f, 3);

			InitializeStateMachine(wanderState, awareState, attackReadyState);

			shoot = new SkeletonArcherShoot().Init(stateMachine);
		}

		protected override Entity GetTargetEntity() => player;

		public override void Attack() {
			entityState = shoot;
		}
	}
}