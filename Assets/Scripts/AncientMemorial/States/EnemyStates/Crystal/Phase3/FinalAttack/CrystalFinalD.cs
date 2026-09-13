using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase3.FinalAttack {
	public class CrystalFinalD : CrystalPhase3Attack {

		// 인스턴스 프로퍼티
		private StopWatch barrageMissileTimer  = new StopWatch();
		private float     barrageMissileTime   = 1f;
		private StopWatch bigMissileTimer      = new StopWatch();
		private float     bigMissileTime       = 1f;

		public override float attackTime => 10f;

		// 인스턴스 메서드
		private void UpdateBarrageMissileTime() {
			barrageMissileTimer.Tick();
			barrageMissileTime = 1f;
		}

		private void UpdateBigMissileTime() {
			bigMissileTimer.Tick();
			bigMissileTime = 1f;
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

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			
			SetMoveMode(CrystalMoveMode.CircleOnCenter);
			
			SpawnRay(4, 3f,    90, effect:true);
			SpawnRay(4, 0.75f, 0);
			SpawnRay(4, 0.75f, 180);
			
			barrageMissileTimer.Tick();
			bigMissileTimer.Tick();
		}

		public override void OnRoutine() {
			base.OnRoutine();
			BarrageMissileRoutine();
			BigMissileRoutine();

			if (isProgress(1)) {
				crystal.entityState = new CrystalFinalE().Init(stateMachine);
			}
		}

		public override void OnExit() {
			base.OnExit();
			ClearRays();
		}
		
	}
}
