using AncientMemorial.Entities.Enemies;
using AncientMemorial.Map;
using UengSystem.UI.UUIs;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase3 {
	public class CrystalSpawnEnergy : CrystalPhase3Attack {

		// 인스턴스 프로퍼티
		public override float attackTime       => 6;
		private         int   spawnEnergyCount = 2;
		private         bool  infoMessage      = true;

		// 인스턴스 메서드
		public CrystalSpawnEnergy Setup(int energyCount) {
			spawnEnergyCount = energyCount;
			return this;
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();

			CrystalPhase3 phase3 = crystal.To<CrystalPhase3>();
			phase3.StopGimmick();
			crystal.To<CrystalPhase3>().StartGimmick(spawnEnergyCount);
			
			crystal.stat.moveSpeed = crystal.data.moveSpeed/1.5f;
			
			if (infoMessage) return;
			infoMessage = false;
			InfoUUI.instance.AddInfoMessage("크리스탈이 <color=#ff5a5a>강력한 공격</color>을 준비합니다. 힘의 파편을 파괴하여 저지하세요.");
			InfoUUI.instance.AddInfoMessage("크리스탈의 <color=#ffea5a>힘의 파편</color>을 파괴하여 크리스탈에게 피해를 입힐 수 있습니다.");
		}
		
		public override void OnRoutine() {
			base.OnRoutine();

			float delay = (float)step / spawnEnergyCount;
			
			if (step < spawnEnergyCount && isProgress(delay)) {
				Vector2 mapSize = MapManager.instance.GetMapSize();

				float   spawnPosX = mapSize.x * (step + 1) / (spawnEnergyCount + 1) - mapSize.x / 2;
				float   spawnPosY = 1.2f                                            + Random.Range(-0.1f, 0.1f);
				Vector2 spawnPos  = new (spawnPosX, spawnPosY);

				crystal.To<CrystalPhase3>().SpawnCrystalEnergy(spawnPos, true);

				step++;
			}

			if (isProgress(1)) {
				crystal.entityState = GetState();
			}
		}
		
		public override void OnExit() {
			base.OnExit();
		}
	}
}
