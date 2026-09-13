using AncientMemorial.Objects;
using UengSystem.Utility;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase3 {
	public class CrystalSemiHugeCross : CrystalPhase3Attack {

		// 정적 프로퍼티
		private const int   MissilesPerWave   = 8;
		private const int   WaveCount         = 4;
		private const int   totalMissileCount = MissilesPerWave * WaveCount;
		private const float FirstShotDelay    = 2f;
		private const float MissileInterval   = 0.1f;
		private const float WaveInterval      = 2f;

		// 인스턴스 프로퍼티
		private CrystalRay ray;
		
		private float waveAngleOffset;
		private float elapsed;
		private readonly StopWatch allMissilesFiredTimer = new();

		public override float attackTime => 11f;

		// 인스턴스 메서드
		private float GetMissileDelay() {
			int previousMissileIndex = (step - 1) % MissilesPerWave;
			
			bool  isFirstShot          = step                 == 0;
			bool  shotAllMissileInWave = previousMissileIndex == MissilesPerWave - 1;
			float delay;
			
			if (isFirstShot)
				delay = FirstShotDelay;
			else if (shotAllMissileInWave)
				delay = WaveInterval;
			else
				delay = MissileInterval;

			return delay;
		}

		private void FinishRoutine() {
			if (step < totalMissileCount) return;
			if (allMissilesFiredTimer.CheckIn(2f)) return;
			
			crystal.entityState = GetState();
		}
		
		private void MainRoutine() {
			int   missileIndex = step % MissilesPerWave;
			float delay        = GetMissileDelay();

			if (stateTimer.CheckIn(elapsed + delay)) return;

			float angle = waveAngleOffset + 360f * missileIndex / MissilesPerWave;
			ShootSemiHugeMissile(crystal.transform.position, angle);
			
			if (missileIndex == MissilesPerWave - 1) {
				waveAngleOffset = UnityEngine.Random.Range(0f, 360f);
			}

			step++;
			elapsed += delay;

			if (step == totalMissileCount) {
				allMissilesFiredTimer.Tick();
			}
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			
			SetMoveMode(CrystalMoveMode.CircleOnCenter);
			waveAngleOffset = UnityEngine.Random.Range(0f, 360f);
			elapsed = 0f;
		}
		
		public override void OnRoutine() {
			if (step < totalMissileCount) {
				MainRoutine();
				return;
			}
			
			FinishRoutine();
		}
	}
}
