using System;
using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.States;
using AncientMemorial.States.PlayerStates;
using AncientMemorial.States.PlayerStates.WeaponAttackState;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Weapons {
	[CreateAssetMenu(fileName = "Shield", menuName = "Weapons/Shield")]
	public class Shield : Weapon {

		// 인스턴스 프로퍼티
		private readonly PlayerWeaponAttack[] primaryAttacks = {
			PlayerWeaponAttack.sweep, 
			PlayerWeaponAttack.strike, 
		};

		// 오버라이드 메서드
		public override void PrimaryAttack() {
			base.PrimaryAttack();
			if (!player.isGround) currPrimaryAttackStage--;
		}

		public override bool CanPrimaryAttack() {
			if (player.entityState == PlayerState.dash) return false;
			return base.CanPrimaryAttack();
		}

		public override bool CanSecondaryAttack() {
			if (player.entityState == PlayerState.dash) return false;
			return base.CanSecondaryAttack();
		}
		
		protected override UState GetPrimaryAttackAction(int attackStage) {
			if (attackStage == -1) return null;
			if (!player.isGround) return PlayerWeaponAttack.fallStrike;
			
			PlayerWeaponAttack attack = primaryAttacks[attackStage];
			return attack;
		}

		protected override UState GetSecondaryAttackAction(int attackStage) {
			return PlayerWeaponAttack.shieldJump;
		}

		protected override float GetPrimaryAttackDelay(int  attackStage) {
			if (attackStage == -1) return 0;

			PlayerWeaponAttack attack = (PlayerWeaponAttack)GetPrimaryAttackAction(attackStage);
			return attack.GetCooldown();
		}
		
		protected override float GetSecondaryAttackDelay(int attackStage) {
			if (attackStage == -1) return 0;
			return PlayerWeaponAttack.shieldJump.GetCooldown();
		}

		protected override void InitializeAttacks() {
			primaryAttackStageCount   = primaryAttacks.Length;
			secondaryAttackStageCount = 1;
		}
	}
}