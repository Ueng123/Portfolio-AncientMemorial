using AncientMemorial.Map;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public class CrystalHugeMissile2 : CrystalPhase2Attack {

		// 인스턴스 프로퍼티
		public override float attackTime => 20f;

		private int spawnCount;
		private int randomizer;
		
		private StopWatch missileTimer     = new StopWatch();
		private float     missileSpawnTime = 0;

		// 인스턴스 메서드
		public void ShootHugeMissileLeft() {
			Vector2 mapSize = MapManager.instance.GetMapSize();

			Vector2 hitboxPos  = new Vector2(-mapSize.x / 4f, mapSize.y / 2);
			Vector2 hitboxSize = new Vector2(mapSize.x  / 2f, mapSize.y);
			
			ShootHugeMissile(hitboxPos, hitboxSize);
		}
		
		public void ShootHugeMissileRight() {
			Vector2 mapSize = MapManager.instance.GetMapSize();

			Vector2 hitboxPos  = new Vector2(mapSize.x / 4f, mapSize.y / 2);
			Vector2 hitboxSize = new Vector2(mapSize.x  / 2f, mapSize.y);
			
			ShootHugeMissile(hitboxPos, hitboxSize);
		}
		
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
			if (step<spawnCount && isProgress(step/10f)) {
				if ((step & 1)==randomizer) {
					ShootHugeMissileLeft();
				}
				else {
					ShootHugeMissileRight();
				}
				step++;
			}

			if (isProgress(1)) {
				crystal.entityState = GetState();
			}
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			spawnCount = Random.Range(9, 12); 
			randomizer = Random.Range(0, 2);
		}
		
		public override void OnRoutine() {
			HugeMissileRoutine();
			MissileRoutine();
		}
	}
}
