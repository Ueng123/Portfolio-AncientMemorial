using UengSystem.Utility;
using AncientMemorial.Cameras;
using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Crystal.Phase1;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.VisualScripting.UValues.UBools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities.Enemies {
	public class CrystalPhase1 : CrystalBoss {

		// 정적 프로퍼티
		private static readonly int SlashEffectPrefabId = "SlashEffect".GetHash();
		private static readonly int CrystalPhase2PrefabId = "CrystalPhase2".GetHash();
		private static readonly int CrystalP2UIPrefabId = "CrystalP2UI".GetHash();
		private static readonly int CrystalSlashClipId = "crystalSlash".GetHash();

		// 인스턴스 프로퍼티
		private EnemyState crystalSpawn;

		// 오버라이드 메서드
		public override void OnFirstGet() {
			base.OnFirstGet();

			InitializeCrystalStateMachine(new CrystalPhase1Idle());
			crystalSpawn = new CrystalSpawn().Init(stateMachine);
		}

		public override void Initialize() {
			base.Initialize();
			
			GameObject sEff      = UObject.Get(SlashEffectPrefabId, transform.position, PlayEffect: false);
			sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
			sEff.transform.localScale = new Vector3(2f, 10f);
			
			GameObject sEff2      = UObject.Get(SlashEffectPrefabId, transform.position, PlayEffect: false);
			sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
			sEff2.transform.localScale = new Vector3(2f, 10f);
			
			CameraBrain.instance.ShakeLerp(5, 10);
			CameraBrain.instance.ZoomLerp(-2f, 3);

			PlaySFX(CrystalSlashClipId);
		}

		protected override Entity GetTargetEntity() => player;

		public override void Attack() {
			entityState = crystalSpawn;
		}

		protected override void Death() {

			CrystalBoss crystal = UObject.Get(CrystalPhase2PrefabId, transform.position, PlayEffect: false).GetComponent<CrystalBoss>();

			if (!string.IsNullOrEmpty(ID)) {
				string id = ID;
				ID         = null;
				crystal.ID = id;
			}
			
			if (TryGetUObject("CrystalBossBar", out UObject bossBar)) {
				bossBar.Release(PlayEffect: true);
				UUI newBossBar = UUI.Get(CrystalP2UIPrefabId, GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
				newBossBar.ID = "CrystalBossBar";
			}
			
			base.Death();
		}
	}
}
