using AncientMemorial.Map;
using UengSystem.States.EnemyStates.Crystal;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public class CrystalPhase2Idle : CrystalState {
		
		private StopWatch smallMissileTimer     = new StopWatch();
		private StopWatch bigMissileTimer     = new StopWatch();
		private float     missileSpawnTime = 0;

		public void UpdateSmallMissileSpawnTime() {
			missileSpawnTime = Random.Range(1f, 2f);
		}
		
		public void UpdateBigMissileSpawnTime() {
			missileSpawnTime = Random.Range(0f, 3f);
		}

		public override void OnEnter() {
			base.OnEnter();
			
			smallMissileTimer.Tick();
			UpdateSmallMissileSpawnTime();

			bigMissileTimer.Tick();
			UpdateBigMissileSpawnTime();
		}

		private void SmallMissileRoutine() {
			if (smallMissileTimer.CheckIn(missileSpawnTime)) return;
			
			Vector2 missilePos   = crystal.transform.position + new Vector3(0, 1.5f, 0);
			float   missileReady = Random.Range(0.3f, 0.5f);
			ShootMissile(missilePos, 0, 13.5f, missileReady, 3f);

			smallMissileTimer.Tick();
			UpdateSmallMissileSpawnTime();
		}
		
		private void BigMissileRoutine() {
			if (bigMissileTimer.CheckIn(missileSpawnTime)) return;
			
			Vector2 mapSize = MapManager.instance.GetMapSize();

			Vector2 missilePos   = new Vector2(Random.Range(-mapSize.x/2f+0.5f, mapSize.x/2f-0.5f), Random.Range(mapSize.y/2f, mapSize.y*5f/6f));
			float   missileReady = Random.Range(0.3f, 0.5f);
			ShootBigMissile(missilePos, Random.Range(0, 360), 13.5f, missileReady, 3f);

			bigMissileTimer.Tick();
			UpdateBigMissileSpawnTime();
		}
		
		public override void OnRoutine() {
			SmallMissileRoutine();
			BigMissileRoutine();
		}
	}
}
