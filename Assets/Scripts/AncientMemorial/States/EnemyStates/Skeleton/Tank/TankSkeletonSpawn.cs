using UengSystem.Utility;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Skeleton.Tank {
	public class TankSkeletonSpawn : SkeletonTankAttack {

		// 정적 프로퍼티
		private static readonly int SkeletonWarriorSpawnPrefabId = "SkeletonWarriorSpawn".GetHash();
		private static readonly int SkeletonArcherSpawnPrefabId = "SkeletonArcherSpawn".GetHash();

		// 인스턴스 프로퍼티
		public override float attackTime => 1.617f;

		// 인스턴스 메서드
		private void SpawnSkeleton() {
			int skeletonName = Random.Range(0, 2) == 0 ? SkeletonWarriorSpawnPrefabId : SkeletonArcherSpawnPrefabId;
			Vector2 spawnPosition = new (enemy.transform.position.x + Random.Range(-2f, 2f), 0.5f);
			
			GameObject spawnObj = UObject.Get(skeletonName, spawnPosition, PlayEffect: false);
			spawnObj.GetComponent<UObject>().Category = "SkeletonSpawnObject";
		}

		// 오버라이드 메서드
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
				enemy.entityState = GetState();
			}
		}
	}
}
