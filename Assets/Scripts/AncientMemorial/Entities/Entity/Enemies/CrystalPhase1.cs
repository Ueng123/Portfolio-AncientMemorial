using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States.EnemyStates;
using UengSystem.States.EnemyStates.Crystal.Phase1;
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

		protected override Entity GetTargetEntity() => player;

		public override void Attack() {
			state = crystalSpawn;
		}

		protected override void Death() {

			CrystalBoss crystal = UObjectPool.instance.Get("CrystalPhase2", transform.position).GetComponent<CrystalBoss>();

			if (!string.IsNullOrEmpty(ID)) {
				string id = ID;
				ID         = null;
				crystal.ID = id;
			}
			
			if (TryGetUObject("CrystalBossBar", out UObject bossBar)) {
				UUIPool.instance.Close(bossBar.gameObject);
				UUI newBossBar = UUIPool.instance.Open("CrystalP2UI", GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
				newBossBar.Category = "CrystalBossBar";
			}
			
			base.Death();
		}
	}
}