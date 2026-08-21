using System.Collections;
using AncientMemorial.Projectiles;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.ObjectPool;
using UengSystem.States.EnemyStates;
using UengSystem.States.EnemyStates.Skeleton;
using UengSystem.States.EnemyStates.Skeleton.Archer;
using UengSystem.States.EnemyStates.Skeleton.Warrior;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class SkeletonArcher : Skeleton {
		private EnemyState shoot;
		
		public override void OnFirstGet() {
			base.OnFirstGet();
			
			EnemyState wanderState      = new SkeletonWander(3, 5);
			EnemyState awareState       = new SkeletonAware(6, 1, 0.5f, -1f, 3);
			EnemyState attackReadyState = new SkeletonAttackReady(6, 1, 0.5f, -1f, 3);

			InitializeStateMachine(wanderState, awareState, attackReadyState);

			shoot = new SkeletonArcherShoot().Init(stateMachine);
		}

		public override Entity GetTargetEntity() => player;

		public override void Attack() {
			state = shoot;
		}
	}
}