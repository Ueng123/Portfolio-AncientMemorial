using AncientMemorial.Map;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public class CrystalHugeMissile1 : CrystalPhase2Attack {

		// 인스턴스 프로퍼티
		public override float attackTime => 12.5f;

		private int[] spawnIndex = new int[] {-1, 0, 1};

		private StopWatch missileTimer     = new StopWatch();
		private float     missileSpawnTime = 0;

		// 인스턴스 메서드
		public void UpdateMissileSpawnTime() {
			missileSpawnTime = Random.Range(0.5f, 1.5f);
		}
		
		private void MissileRoutine() {
			if (missileTimer.CheckIn(missileSpawnTime)) return;
			
			Vector2 mapSize = MapManager.instance.GetMapSize();

			Vector2 missilePos   = new Vector2(Random.Range(-mapSize.x/2f+0.5f, mapSize.x/2f-0.5f), Random.Range(mapSize.y/2f, mapSize.y*5f/6f));
			if (Random.Range(0, 1) == 1) {
				ShootMissile(missilePos, Random.Range(0, 360), 13.5f, 0, 3f);
			}
			else {
				ShootBigMissile(missilePos, Random.Range(0, 360), 13.5f, 0, 3f);
			}

			missileTimer.Tick();
			UpdateMissileSpawnTime();
		}

		private void HugeMissileRoutine() {
			if (step == 0 && isProgress(0.1f)) {
				ShootHugeMissile(spawnIndex[0]);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(0.3f)) {
				ShootHugeMissile(spawnIndex[1]);
				
				step = 2;
			}
			
			if (step == 2 && isProgress(0.5f)) {
				ShootHugeMissile(spawnIndex[2]);
				
				step = 3;
			}
			
			if (step == 3 && isProgress(1f)) {
				crystal.entityState = GetState();
			}
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			spawnIndex.Shuffle();
		}

		public override void OnRoutine() {
			HugeMissileRoutine();
			MissileRoutine();
		}
	}
}
