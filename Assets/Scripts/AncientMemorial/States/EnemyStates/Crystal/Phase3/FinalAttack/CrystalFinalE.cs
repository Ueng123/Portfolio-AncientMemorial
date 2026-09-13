using UengSystem.Utility;
using UengSystem.Audio;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase3.FinalAttack {
	public class CrystalFinalE : CrystalPhase3Attack {

		// 정적 프로퍼티
		private static readonly int CrystalDeathClipId = "crystalDeath".GetHash();

		// 인스턴스 프로퍼티
		public override float attackTime => 10f;

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();

			SetMoveMode(CrystalMoveMode.Death);
			crystal.animator.Play("Death");
			AudioManager.instance.PlaySFX(CrystalDeathClipId);
			
			SpawnRay(-3.5f, 0, 30, effect:true);
			SpawnRay(-3.5f, 0, 90);
			SpawnRay(-3.5f, 0, 150);
			SpawnRay(-3.5f, 0, 210);
			SpawnRay(-3.5f, 0, 270);
			SpawnRay(-3.5f, 0, 330);
		}

		public override void OnRoutine() {
			base.OnRoutine();
			
			if (isProgress(1)) {
				stateMachine.AttackWatchTick();
				crystal.entityState = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();
			ClearRays();

			crystal.stat.HP = 0;
		}
	}
}
