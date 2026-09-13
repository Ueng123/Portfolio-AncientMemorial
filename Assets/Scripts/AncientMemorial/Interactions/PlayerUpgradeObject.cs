using UengSystem.UI;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UengSystem.UI.UUIs;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UBools;
using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;

namespace AncientMemorial.Interactions {
	public class PlayerUpgradeObject : Interaction {

		// 정적 프로퍼티
		private static readonly int UPGRADE_DONE     = "UpgradeDone".GetHash();
		private static readonly int AD_UPGRADE_COUNT = "ADUpgradeCount".GetHash();
		private static readonly int AS_UPGRADE_COUNT = "ASUpgradeCount".GetHash();
		private static readonly int MS_UPGRADE_COUNT = "MSUpgradeCount".GetHash();
		private static readonly int MH_UPGRADE_COUNT = "MHUpgradeCount".GetHash();

		// 인스턴스 프로퍼티
		private bool                  selected = false;
		
		public string         upgradeSuccessMessage;
		public float          upgradeAmount;
		public EntityDataType upgradeStat;

		// 인스턴스 메서드
		private void Upgrade() {
			InfoUUI.instance.AddInfoMessage(upgradeSuccessMessage);
			UPureBool.SetValue(UPGRADE_DONE, true);
			
			switch (upgradeStat) {
				case EntityDataType.AttackDamage:
					UPureFloat.AddValue(AD_UPGRADE_COUNT, 1);
					Entity.player.stat.attackDamage += upgradeAmount;
					break;
				case EntityDataType.AttackSpeed:
					UPureFloat.AddValue(AS_UPGRADE_COUNT, 1);
					Entity.player.stat.attackSpeed += upgradeAmount;
					break;
				case EntityDataType.MoveSpeed:
					UPureFloat.AddValue(MS_UPGRADE_COUNT, 1);
					Entity.player.stat.moveSpeed += upgradeAmount;
					break;
				case EntityDataType.MAXHP:
					UPureFloat.AddValue(MH_UPGRADE_COUNT, 1);
					Entity.player.data.HP += upgradeAmount;
					Entity.player.stat.HP += upgradeAmount;
					break;
			}
		}

		// 오버라이드 메서드
		protected override void OnInteract() {
			Upgrade();
			
			selected             = true;
			List<UObject> playerUpgradeObjects = GetUObjects(Category);

			for (int i = playerUpgradeObjects.Count - 1; i >= 0; i--) {
				UObject upgradeObject = playerUpgradeObjects[i];
				upgradeObject.Release(PlayEffect: true);
			}
		}

		public override void OnFirstGet() {
			SetDefaultStates(new AnimationGetting(this, "spawn", "idle", 2), new UpgradeReleasing(this));
			base.OnFirstGet();
		}

		public override void OnGet() {
			selected = false;
			base.OnGet();
		}

		// 중첩 타입
		private sealed class UpgradeReleasing : Releasing {

			// 인스턴스 프로퍼티
			private float InitialSpeed;

			// 인스턴스 메서드
			public UpgradeReleasing(PlayerUpgradeObject Target) : base(Target) { }

			// 오버라이드 메서드
			protected override void OnStartEffect() {
				InitialSpeed = target.animator.speed;
				target.animator.speed = duration > 0 ? 1 / duration : 1;
				target.animator.Play(((PlayerUpgradeObject)target).selected ? "select" : "break", 0, 0);
			}
			protected override void ClearEffect() {
				if (hasStartedEffect && target.animator) target.animator.speed = InitialSpeed;
				((PlayerUpgradeObject)target).selected = false;
			}
		}
	}
}
