using System;
using AncientMemorial.Entities.Enemies;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UengSystem.States.EnemyStates.Crystal.Phase1 {
	public class CrystalPhase1Idle : CrystalState {
		
		private StopWatch missileTimer     = new StopWatch();
		private float     missileSpawnTime = 0;

		public void UpdateMissileSpawnTime() {
			missileSpawnTime = Random.Range(0.5f, 1.5f);
		}

		public override void OnEnter() {
			base.OnEnter();
			
			missileTimer.Tick();
			UpdateMissileSpawnTime();
		}

		public override void OnRoutine() {
			if (missileTimer.CheckIn(missileSpawnTime)) return;
			
			Vector2 missilePos   = crystal.transform.position + new Vector3(0, 1.5f, 0);
			float   missileReady = Random.Range(0.3f, 0.5f);
			ShootMissile(missilePos, 0, 13.5f, missileReady, 3f);

			missileTimer.Tick();
			UpdateMissileSpawnTime();
		}
	}
}
