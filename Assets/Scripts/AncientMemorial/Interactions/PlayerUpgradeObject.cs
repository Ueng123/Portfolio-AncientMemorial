using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI.UUIs;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UBools;
using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;

namespace AncientMemorial.Interactions {
	public class PlayerUpgradeObject : Interaction {
		private static readonly int UPGRADE_DONE     = "UpgradeDone".GetHash();
		private static readonly int AD_UPGRADE_COUNT = "ADUpgradeCount".GetHash();
		private static readonly int AS_UPGRADE_COUNT = "ASUpgradeCount".GetHash();
		private static readonly int MS_UPGRADE_COUNT = "MSUpgradeCount".GetHash();
		private static readonly int MH_UPGRADE_COUNT = "MHUpgradeCount".GetHash();
		
		private bool                  selected = false;
		
		public string         upgradeSuccessMessage;
		public float          upgradeAmount;
		public EntityDataType upgradeStat;

		private void Upgrade() {
			InfoUUI.instance.AddInfoMessage(upgradeSuccessMessage);
			UPureBool.SetValue(UPGRADE_DONE, true);
			
			switch (upgradeStat) {
				case EntityDataType.AttackDamage:
					UPureFloat.AddValue(AD_UPGRADE_COUNT, 1);
					Entity.player.entityStat.attackDamage += upgradeAmount;
					break;
				case EntityDataType.AttackSpeed:
					UPureFloat.AddValue(AS_UPGRADE_COUNT, 1);
					Entity.player.entityStat.attackSpeed += upgradeAmount;
					break;
				case EntityDataType.MoveSpeed:
					UPureFloat.AddValue(MS_UPGRADE_COUNT, 1);
					Entity.player.entityStat.moveSpeed += upgradeAmount;
					break;
				case EntityDataType.MAXHP:
					UPureFloat.AddValue(MH_UPGRADE_COUNT, 1);
					Entity.player.entityData.HP += upgradeAmount;
					Entity.player.entityStat.HP += upgradeAmount;
					break;
			}
		}
		
		protected override void OnInteract() {
			Upgrade();
			
			selected             = true;
			List<UObject> playerUpgradeObjects = GetUObjects(Category);

			for (int i = playerUpgradeObjects.Count - 1; i >= 0; i--) {
				UObject upgradeObject = playerUpgradeObjects[i];
				UObjectPool.instance.Release(upgradeObject.gameObject, 1f);
			}
		}

		protected override void PrepareSpawnFX() { }
		
		protected override IEnumerator SpawnFX(float duration) {
			animator.Play("spawn");
			animator.speed = 2f/duration;
			
			yield return new WaitForSeconds(duration);
		}

		protected override void        FinishSpawnFX() {
			animator.Play("idle");
			animator.speed = 1;
			
			Initialize();
		}

		protected override void        PrepareDespawnFX() { }
		protected override IEnumerator DespawnFX(float duration) {
			animator.Play(selected?"select":"break");
			animator.speed = 1f/duration;
			selected       = false;
			
			yield return new WaitForSeconds(duration);
			
			UObjectPool.instance.Release(gameObject, -1);
		}
	}
}