using System;
using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States;
using UengSystem.States.PlayerStates.WeaponAttackState;
using UengSystem.Utility;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;
using Random = UnityEngine.Random;

namespace AncientMemorial.Weapons {
	[CreateAssetMenu(fileName = "Crossbow", menuName = "Weapons/Crossbow")]
	public class Crossbow : Weapon {

		private bool          canFallAttack = false;
		private Action<Event> onPlayerLand;
		private void OnPlayerLand(Event e) {
			canFallAttack = true;
		}

		public override bool CanPrimaryAttack() {
			if (!player.isGround && !canFallAttack) return false;
			return base.CanPrimaryAttack();
		}

		public override void PrimaryAttack() {
			base.PrimaryAttack();
			if (!player.isGround) {
				canFallAttack = false;
				currPrimaryAttackStage--;
			}
		}

		private readonly PlayerWeaponAttack[] primaryAttacks = {
			PlayerWeaponAttack.shoot1, 
			PlayerWeaponAttack.shoot2, 
			PlayerWeaponAttack.shoot3, 
			PlayerWeaponAttack.shoot3,
			PlayerWeaponAttack.shoot5,
		};
		
		protected override UState GetPrimaryAttackAction(int attackStage) {
			if (attackStage == -1) return null;
			if (!player.isGround) return PlayerWeaponAttack.fallShoot;
			
			PlayerWeaponAttack attack = primaryAttacks[attackStage];
			return attack;
		}

		protected override UState GetSecondaryAttackAction(int attackStage) {
			return PlayerWeaponAttack.shootBoost;
		}
		
		protected override float GetPrimaryAttackDelay(int  attackStage) {
			if (attackStage == -1) return 0;

			PlayerWeaponAttack attack = (PlayerWeaponAttack)GetPrimaryAttackAction(attackStage);
			return attack.GetCooldown();
		}

		protected override float GetSecondaryAttackDelay(int attackStage) {
			if (attackStage == -1) return 0;
			return PlayerWeaponAttack.shootBoost.GetCooldown();
		}

		protected override void InitializeAttacks() {
			primaryAttackStageCount   = primaryAttacks.Length;
			secondaryAttackStageCount = 1;
			
			onPlayerLand ??= OnPlayerLand;
			
			EventType.Entity_Player_Land.AddListener(onPlayerLand);
		}

		public override void Uninitialize() {
			
		}
	}
}