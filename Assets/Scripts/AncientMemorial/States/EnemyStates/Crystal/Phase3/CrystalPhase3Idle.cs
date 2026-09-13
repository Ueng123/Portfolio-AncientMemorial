using AncientMemorial.Map;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase3 {
	public class CrystalPhase3Idle : CrystalPhase3State {

		// 인스턴스 프로퍼티
		private StopWatch fallingMissileTimer = new StopWatch();
		private float     fallingMissileTime  = 0.25f;
		
		private StopWatch semiHugeMissileTimer = new StopWatch();
		private float     semiHugeMissileTime  = 3f;

		// 인스턴스 메서드
		private void UpdateFallingMissileTime() {
			fallingMissileTimer.Tick();
			fallingMissileTime = Random.Range(0.1f, 0.3f);
		}
		
		private void UpdateSemiHugeMissileTime() {
			semiHugeMissileTimer.Tick();
			semiHugeMissileTime = Random.Range(2.8f, 3.2f);
		}
		
		private void FallingMissileRoutine() {
			if (fallingMissileTimer.CheckIn(fallingMissileTime)) return;
			UpdateFallingMissileTime();
			
			Vector2 mapSize = MapManager.instance.GetMapSize();
			
			Vector2 pos   = new (Random.Range(-(mapSize.x / 2 - 1), mapSize.x / 2 - 1), mapSize.y - 1);
			float   rot   = Random.Range(175f, 185f);
			float   speed = Random.Range(12f, 15f);
			
			ShootBigMissile(pos, rot, speed);
		}

		private void SemiHugeMissileRoutine() {
			if (semiHugeMissileTimer.CheckIn(semiHugeMissileTime)) return;
			UpdateSemiHugeMissileTime();

			Vector2 pos   = crystal.transform.position;
			float   rot   = GetAngleToTarget(pos, true);
			float   speed = Random.Range(6f, 7f);

			ShootSemiHugeMissile(pos, rot, speed);
			ShootBarrageMissile(pos, Random.Range(7, 10));
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			SetMoveMode(CrystalMoveMode.LissajousPath);
			
			fallingMissileTimer.Tick();
			semiHugeMissileTimer.Tick();
		}
		
		public override void OnRoutine() {
			base.OnRoutine();
			
			FallingMissileRoutine();
			SemiHugeMissileRoutine();
		}
	}
}
