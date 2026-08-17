using System;
using UengSystem.Logic.Tasks;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class SpawnEnemy : TaskComponent {
		public Vector2      spawnRangeX;
		public Vector2      spawnRangeY;
		public float        spawnTime;
		public GameObject[] enemyPrefab;
		public override void       Execute(ITaskable self) {
			GameObject entity = UObjectPool.instance.Get(enemyPrefab[Random.Range(0, enemyPrefab.Length)].name, new Vector2(
										 Random.Range(spawnRangeX.x, spawnRangeX.y),
										 Random.Range(spawnRangeY.x, spawnRangeY.y)),
									 spawnTime);

			entity.GetComponent<UObject>().Category = "Enemy";
		}
	}
}