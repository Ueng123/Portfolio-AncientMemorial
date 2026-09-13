using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase1 {
	public class CrystalPhase1Idle : CrystalState {

		// 인스턴스 프로퍼티
		private StopWatch missileTimer     = new StopWatch();
		private float     missileSpawnTime = 0;

		// 인스턴스 메서드
		public void UpdateMissileSpawnTime() {
			missileSpawnTime = Random.Range(0.5f, 1.5f);
		}

		// 오버라이드 메서드
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
