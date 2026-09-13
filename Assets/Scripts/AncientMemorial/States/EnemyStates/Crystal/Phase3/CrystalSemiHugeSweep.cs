using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase3 {
	public class CrystalSemiHugeSweep : CrystalPhase3Attack {

		// 인스턴스 프로퍼티
		public override float attackTime => 10;

		private StopWatch missileTimer = new StopWatch();
		private float     missileSpawnTime;

		private StopWatch barrageMissileTimer = new StopWatch();
		private float     barrageMissileTime;

		private float oldSpeed;

		// 인스턴스 메서드
		private void UpdateMissileSpawnTime() {
			missileTimer.Tick();
			missileSpawnTime = 0.25f;
		}

		private void UpdateBarrageMissileTime() {
			barrageMissileTimer.Tick();
			barrageMissileTime = Random.Range(1f, 1.25f);
		}

		private void MissileRoutine() {
			if (missileTimer.CheckIn(missileSpawnTime)) return;
			UpdateMissileSpawnTime();

			ShootSemiHugeMissile(crystal.transform.position, Random.Range(160f, 200f));
		}

		private void BarrageMissileRoutine() {
			if (barrageMissileTimer.CheckIn(barrageMissileTime)) return;
			UpdateBarrageMissileTime();

			ShootBarrageMissile(crystal.transform.position, Random.Range(10, 15));
		}
		
		private CrystalMoveMode GetMoveMode() {
			return crystal.transform.position.x > 0 ? CrystalMoveMode.CurveFromLToR : CrystalMoveMode.CurveFromRToL;
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			oldSpeed = crystal.stat.moveSpeed;
			crystal.stat.moveSpeed = crystal.data.moveSpeed * 3;

			UpdateMissileSpawnTime();
			UpdateBarrageMissileTime();
		}
		
		public override void OnRoutine() {
			if (step != 3) {
				MissileRoutine();
				BarrageMissileRoutine();
			}

			if (step == 0 && isProgress(0)) {
				SetMoveMode(GetMoveMode());
				step = 1;
			}
			
			if (step == 1 && isProgress(0.35f)) {
				SetMoveMode(GetMoveMode());
				step = 2;
			}
			
			if (step == 2 && isProgress(0.7f)) {
				SetMoveMode(CrystalMoveMode.LissajousPath);
				step = 3;
			}

			if (step == 3 && isProgress(1f)) {
				crystal.entityState = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();
			crystal.stat.moveSpeed = oldSpeed;
		}
	}
}
