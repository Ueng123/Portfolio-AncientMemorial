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
		private EnemyState crystalSpawn;
		
		public override void OnFirstGet() {
			base.OnFirstGet();

			InitializeCrystalStateMachine(new CrystalPhase1Idle());
			crystalSpawn = new CrystalSpawn().Init(stateMachine);
		}

		public override void Initialize() {
			base.Initialize();
			
			GameObject sEff      = UObject.Get("SlashEffect", transform.position, PlayEffect: false);
			sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
			sEff.transform.localScale = new Vector3(2f, 10f);
			
			GameObject sEff2      = UObject.Get("SlashEffect", transform.position, PlayEffect: false);
			sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
			sEff2.transform.localScale = new Vector3(2f, 10f);
			
			CameraBrain.instance.ShakeLerp(5, 10);
			CameraBrain.instance.ZoomLerp(-2f, 3);

			PlaySFX("crystalSlash");
		}

		protected override Entity GetTargetEntity() => player;

		public override void Attack() {
			entityState = crystalSpawn;
		}

		protected override void Death() {

			CrystalBoss crystal = UObject.Get("CrystalPhase2", transform.position, PlayEffect: false).GetComponent<CrystalBoss>();

			if (!string.IsNullOrEmpty(ID)) {
				string id = ID;
				ID         = null;
				crystal.ID = id;
			}
			
			if (TryGetUObject("CrystalBossBar", out UObject bossBar)) {
				bossBar.Release(PlayEffect: true);
				UUI newBossBar = UUI.Get("CrystalP2UI", GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
				newBossBar.ID = "CrystalBossBar";
			}
			
			base.Death();
		}
	}
}
