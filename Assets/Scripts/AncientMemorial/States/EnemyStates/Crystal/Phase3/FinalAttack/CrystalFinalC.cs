namespace UengSystem.States.EnemyStates.Crystal.Phase3 {
	using AncientMemorial.Objects;
	using UengSystem.Utility;
	using UnityEngine;

	public class CrystalFinalC : CrystalPhase3Attack {
		private StopWatch barrageMissileTimer  = new StopWatch();
		private float     barrageMissileTime   = 1f;
		private StopWatch bigMissileTimer      = new StopWatch();
		private float     bigMissileTime       = 1f;

		private void UpdateBarrageMissileTime() {
			barrageMissileTimer.Tick();
			barrageMissileTime = 1f;
		}

		private void UpdateBigMissileTime() {
			bigMissileTimer.Tick();
			bigMissileTime = 2f;
		}

		private void BarrageMissileRoutine() {
			if (barrageMissileTimer.CheckIn(barrageMissileTime)) return;
			UpdateBarrageMissileTime();
			ShootBarrageMissile(crystal.transform.position, 15);
		}

		private void BigMissileRoutine() {
			if (bigMissileTimer.CheckIn(bigMissileTime)) return;
			UpdateBigMissileTime();

			Vector2 pos = crystal.transform.position;
			float   rot = GetAngleToTarget(pos, true);
			ShootBigMissile(pos, rot);
		}
		public override void OnEnter() {
			base.OnEnter();
			
			SetMoveMode(CrystalMoveMode.CircleOnCenter);
			
			SpawnRay(4, 1.5f, 45, effect:true);
			SpawnRay(4, 1.5f, 135);
			SpawnRay(4, 1.5f, 215);
			SpawnRay(4, 1.5f, 305);
			
			barrageMissileTimer.Tick();
			bigMissileTimer.Tick();
		}

		public override void OnRoutine() {
			base.OnRoutine();
			BarrageMissileRoutine();
			BigMissileRoutine();

			if (isProgress(1)) {
				crystal.state = new CrystalFinalD().Init(stateMachine);
			}
		}

		public override void OnExit() {
			base.OnExit();
			ClearRays();
		}

		public override float attackTime => 10f;
		
	}
}
