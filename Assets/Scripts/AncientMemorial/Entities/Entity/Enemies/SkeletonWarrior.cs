using System.Collections;
using System.Linq;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.States.EnemyStates;
using UengSystem.States.EnemyStates.Skeleton;
using UengSystem.States.EnemyStates.Skeleton.Warrior;
using UengSystem.Utility;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace AncientMemorial.Entities {
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

		public override Entity GetTargetEntity() => player;

		public override void Attack() {
			state = pierce;
		}
	}
}