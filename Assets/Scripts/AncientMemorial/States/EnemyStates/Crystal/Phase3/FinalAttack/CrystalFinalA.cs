using AncientMemorial.Map;

namespace UengSystem.States.EnemyStates.Crystal.Phase3 {
	using AncientMemorial.Objects;
	using UengSystem.Utility;
	using UnityEngine;

	public class CrystalFinalA : CrystalPhase3Attack {
		
		public override float attackTime => 10f;
		
		private StopWatch barrageMissileTimer  = new StopWatch();
		private float     barrageMissileTime   = 1f;
		private StopWatch semiHugeMissileTimer = new StopWatch();
		private float     semiHugeMissileTime  = 1f;
		private StopWatch bigMissileTimer      = new StopWatch();
		private float     bigMissileTime       = 1f;

		private void UpdateBarrageMissileTime() {
			barrageMissileTimer.Tick();
			barrageMissileTime = 1.5f;
		}

		private void UpdateSemiHugeMissileTime() {
			semiHugeMissileTimer.Tick();
			semiHugeMissileTime = Random.Range(0.25f, 0.75f);
		}

		private void UpdateBigMissileTime() {
			bigMissileTimer.Tick();
			bigMissileTime = 1.5f;
		}

		private void BarrageMissileRoutine() {
			if (barrageMissileTimer.CheckIn(barrageMissileTime)) return;
			UpdateBarrageMissileTime();
			ShootBarrageMissile(crystal.transform.position, 10);
		}

		private void SemiHugeMissileRoutine() {
			if (semiHugeMissileTimer.CheckIn(semiHugeMissileTime)) return;
			UpdateSemiHugeMissileTime();

			Vector2 mapSize = MapManager.instance.GetTargetMapSize();
			float   posX    = Random.Range(-0.45f, 0.45f) * mapSize.x;
			float   posY    = mapSize.y                   * 0.8f;
			Vector2 pos     = new (posX, posY);
			
			ShootSemiHugeMissile(pos, Random.Range(175f, 185f));
		}

		private void BigMissileRoutine() {
			if (bigMissileTimer.CheckIn(bigMissileTime)) return;
			UpdateBigMissileTime();

			Vector2 pos = crystal.transform.position;
			float   rot = GetAngleToTarget(pos, true);
			ShootBigMissile(pos, rot);
		}
		public override void OnEnter() {
			MapManager.instance.SetMapSize(new Vector2(15, 15), 0.025f);
			SetMoveMode(CrystalMoveMode.CircleOnCenter);
			
			base.OnEnter();

			SpawnRay(2, 0, 90, effect:true);
			SpawnRay(2, 0, 270);
			
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
				crystal.state = new CrystalFinalB().Init(stateMachine);
			}
		}

		public override void OnExit() {
			base.OnExit();
			ClearRays();
		}
	}
}
