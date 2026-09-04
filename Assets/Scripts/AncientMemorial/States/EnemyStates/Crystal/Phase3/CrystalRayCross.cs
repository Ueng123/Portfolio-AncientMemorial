using UnityEngine;

using UengSystem.Utility;

namespace UengSystem.States.EnemyStates.Crystal.Phase3 {
	public class CrystalRayCross : CrystalPhase3Attack {
		public override float attackTime => 10f;

		private StopWatch bigMissileTimer     = new StopWatch();
		private float     bigMissileTime      = 1;
		private StopWatch barrageMissileTimer = new StopWatch();
		private float     barrageMissileTime  = 1f;

		private void UpdateBigMissileTime() {
			bigMissileTimer.Tick();
			bigMissileTime = 1;
		}

		private void UpdateBarrageMissileTime() {
			barrageMissileTimer.Tick();
			barrageMissileTime = 1f;
		}

		private void BigMissileRoutine() {
			if (bigMissileTimer.CheckIn(bigMissileTime)) return;
			UpdateBigMissileTime();

			Vector2 pos = crystal.transform.position;
			float   rot = GetAngleToTarget(pos, true);

			ShootBigMissile(pos, rot+Random.Range(-10f, 10f));
		}

		private void BarrageMissileRoutine() {
			if (barrageMissileTimer.CheckIn(barrageMissileTime)) return;
			UpdateBarrageMissileTime();

			ShootBarrageMissile(crystal.transform.position, 10);
		}

		public override void OnEnter() {
			base.OnEnter();
			SetMoveMode(CrystalMoveMode.CircleOnCenter);
			
			bigMissileTimer.Tick();
			barrageMissileTimer.Tick();
			SpawnRay(4, 1f, 0, effect:true);
			SpawnRay(4, 1f, 90);
			SpawnRay(4, 1f, 180);
			SpawnRay(4, 1f, 270);
		}
		
		public override void OnRoutine() {
			base.OnRoutine();
			BigMissileRoutine();
			BarrageMissileRoutine();

			if (step == 0 && isProgress(0.8f)) {
				ClearRays();
				step = 1;
			}

			if (step == 1 && isProgress(1)) {
				crystal.state = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();
			ClearRays();
		}
	}
}
