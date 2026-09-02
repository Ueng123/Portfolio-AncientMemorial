namespace UengSystem.States.EnemyStates.Crystal.Phase3 {
	using AncientMemorial.Objects;
	using UengSystem.Utility;
	using UnityEngine;

	public class CrystalFinalD : CrystalPhase3Attack {
		private StopWatch barrageMissileTimer  = new StopWatch();
		private float     barrageMissileTime   = 1f;
		private StopWatch semiHugeMissileTimer = new StopWatch();
		private float     semiHugeMissileTime  = 1f;
		private StopWatch bigMissileTimer      = new StopWatch();
		private float     bigMissileTime       = 1f;

		private void UpdateBarrageMissileTime() {
			barrageMissileTimer.Tick();
			barrageMissileTime = 1f;
		}

		private void UpdateSemiHugeMissileTime() {
			semiHugeMissileTimer.Tick();
			semiHugeMissileTime = 1f;
		}

		private void UpdateBigMissileTime() {
			bigMissileTimer.Tick();
			bigMissileTime = 1f;
		}

		private void BarrageMissileRoutine() {
			if (barrageMissileTimer.CheckIn(barrageMissileTime)) return;
			UpdateBarrageMissileTime();
			ShootBarrageMissile(crystal.transform.position, 1);
		}

		private void SemiHugeMissileRoutine() {
			if (semiHugeMissileTimer.CheckIn(semiHugeMissileTime)) return;
			UpdateSemiHugeMissileTime();
			ShootSemiHugeMissile(crystal.transform.position, Random.Range(0f, 360f));
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
			barrageMissileTimer.Tick();
			semiHugeMissileTimer.Tick();
			bigMissileTimer.Tick();
		}

		public override void OnRoutine() {
			base.OnRoutine();
			BarrageMissileRoutine();
			SemiHugeMissileRoutine();
			BigMissileRoutine();

			if (isProgress(1)) {
				crystal.state = new CrystalFinalE().Init(stateMachine);
			}
		}

		public override void OnExit() {
			base.OnExit();
			ClearRays();
		}

		public override float attackTime => 0f;
		
	}
}
