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
	[CreateAssetMenu(fileName = "Sword", menuName = "Weapons/Sword")]
	public class Sword : Weapon {

		// 정적 프로퍼티
		private const float skillEndCooldown = 25f;

		// 인스턴스 프로퍼티
		private readonly PlayerWeaponAttack[] primaryAttacks = {
			PlayerWeaponAttack.slash1, 
			PlayerWeaponAttack.slash2, 
			PlayerWeaponAttack.slash3,
		};

		// 오버라이드 메서드
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
			if (!player.isGround) attackStage = 1;

			PlayerWeaponAttack attack = primaryAttacks[attackStage];
			return attack;
		}

		protected override UState GetSecondaryAttackAction(int attackStage) {
			return PlayerWeaponAttack.flashSlash;
		}

		protected override float GetPrimaryAttackDelay(int  attackStage) {
			if (attackStage == -1) return 0;
			if (!player.isGround) attackStage = 1;

			PlayerWeaponAttack attack = primaryAttacks[attackStage];
			return attack.GetCooldown();
		}

		protected override float GetSecondaryAttackDelay(int attackStage) {
			return attackStage switch {
				-1 => 0f,
				0  => 0.5f,
				1 => 0.5f,
				2  => Mathf.Max(0.5f, skillEndCooldown  / player.stat.attackSpeed),
				_  => throw new ArgumentOutOfRangeException(nameof(attackStage), attackStage, null)
			};
		}

		protected override void InitializeAttacks() {
			primaryAttackStageCount   = primaryAttacks.Length;
			secondaryAttackStageCount = 3;
		}
	}
}