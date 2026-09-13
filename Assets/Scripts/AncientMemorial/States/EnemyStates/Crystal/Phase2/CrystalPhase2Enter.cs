using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Cameras;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public class CrystalPhase2Enter : CrystalPhase2State {

		// 정적 프로퍼티
		private static readonly int SlashEffectPrefabId = "SlashEffect".GetHash();
		private static readonly int CrystalSlashClipId = "crystalSlash".GetHash();

		// 인스턴스 프로퍼티
		private float duration = 10f;

		// 오버라이드 메서드
		public override void OnEarlyRoutine() { }
		
		public override void OnRoutine() {
			base.OnRoutine();
			crystal.transform.Translate(Vector3.up*(Time.deltaTime/5f));

			if (stateTimer.CheckOut(duration)) crystal.entityState = GetState();
		}
		
		public override void OnExit() {
			base.OnExit();
			
			GameObject sEff      = UObject.Get(SlashEffectPrefabId, crystal.transform.position, PlayEffect: false);
			sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
			sEff.transform.localScale = new Vector3(2f, 10f);
			
			GameObject sEff2      = UObject.Get(SlashEffectPrefabId, crystal.transform.position, PlayEffect: false);
			sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
			sEff2.transform.localScale = new Vector3(2f, 10f);
			
			CameraBrain.instance.ShakeLerp(5, 10);
			CameraBrain.instance.ZoomLerp(-2f, 3);
			
			crystal.PlaySFX(CrystalSlashClipId);
		}
	}
}
