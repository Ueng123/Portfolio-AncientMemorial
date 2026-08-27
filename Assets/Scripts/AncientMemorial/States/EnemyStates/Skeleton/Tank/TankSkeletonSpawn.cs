using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Skeleton.Tank {
	public class TankSkeletonSpawn : SkeletonTankAttack {
		public override float attackTime => 1.617f;

		private void SpawnSkeleton() {
			string  skeletonName  = Random.Range(0, 2) == 0 ? "SkeletonWarriorSpawn" : "SkeletonArcherSpawn";
			Vector2 spawnPosition = new (enemy.transform.position.x + Random.Range(-2f, 2f), 0.5f);
			
			GameObject spawnObj = UObjectPool.instance.Get(skeletonName, spawnPosition);
			spawnObj.GetComponent<UObject>().Category = "SkeletonSpawnObject";
		}

		public override void OnRoutine() {
			if (step==0&&isProgress(0)) {
				SpawnSkeleton();
				step++;
			}
			
			if (step==1&&isProgress(1/3f)) {
				SpawnSkeleton();
				step++;
			}
			
			if (step==2&&isProgress(2/3f)) {
				SpawnSkeleton();
				step++;
			}
			
			if (step==3&&isProgress(1)) {
				SpawnSkeleton();
				step++;
			}
		}
	}
}