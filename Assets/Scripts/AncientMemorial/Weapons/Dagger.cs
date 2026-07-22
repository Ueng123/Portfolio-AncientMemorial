using System;
using System.Collections;
using AncientMemorial.Entities;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Weapons {
	[CreateAssetMenu(fileName = "Dagger", menuName = "Weapons/Dagger")]
	public class Dagger : Weapon {

		public override void Attack() {
			if (!player.isGround) {
				if (!isCancellable) return;
				currAttackStage = 0;
				base.Attack();
			};
			base.Attack();
		}

		private float oldAnimatorSpeed;

		private DelayedAction attackDelayedAction;
		private Vector2       plrPos => player.transform.position;
		
		private const float attackLength1 = 0.833f;
		private IEnumerator attack3Enumerator() {
			Vector2 mousePos = InputManager.inputData[InputActionType.MousePosition].valueV;
			bool    isBack   = mousePos.x < player.transform.position.x;
			
			player.animator.Play("Dattack1"+(isBack?"B":""), 0);
			
			Vector2 hitboxPos  = new Vector2(0.6f*(isBack?-1:1), -0.1f);
			Vector2 hitboxSize = new Vector2(1f, 0.7f);
			
			oldAnimatorSpeed = player.animator.speed;
			player.animator.speed = player.entityStat.attackSpeed;
			
			yield return new WaitForSeconds(attackLength1 * 0.1f/player.entityStat.attackSpeed);
			
			GameObject obj = UObjectPool.instance.Get("SlashEffect", (Vector2)player.transform.position + new Vector2(0.48f*(isBack?-1:1)+Random.Range(-0.1f, 0.1f),Random.Range(-0.1f, 0.1f)));
			obj.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
			obj.transform.localScale = new Vector3(0.9f, 1.2f);
			
			// damage
			const int maxTargetEntity = 2;
			Entity.AttackAreaNoEffect(player, 1.3f, 0, plrPos+hitboxPos, hitboxSize, 0, maxTargetEntity);
			player.PlaySFX("swordSlash3");
			
			yield return new WaitForSeconds(attackLength1 * 0.9f/player.entityStat.attackSpeed);
		}

		private const float attackLength2 = 0.85f;
		private IEnumerator attack1Enumerator() {
			Vector2 mousePos = InputManager.inputData[InputActionType.MousePosition].valueV;
			bool    isBack   = mousePos.x < player.transform.position.x;
			
			player.animator.Play("Dattack2"+(isBack?"B":""), 0);
			
			Vector2 hitboxPos  = new Vector2(1f*(isBack?-1:1), -0.09f);
			Vector2 hitboxSize = new Vector2(1.5f,               0.3f);
			
			oldAnimatorSpeed      = player.animator.speed;
			player.animator.speed = player.entityStat.attackSpeed;
			
			yield return new WaitForSeconds(attackLength1 * 0.06f/player.entityStat.attackSpeed);
			
			GameObject obj = UObjectPool.instance.Get("SlashEffect", (Vector2)player.transform.position + new Vector2(0.6f*(isBack?-1:1)+Random.Range(-0.2f, 0.2f),-0.1f+Random.Range(-0.2f, 0.2f)));
			obj.transform.rotation   = Quaternion.Euler(0, 0, 90 + Random.Range(-1f, 1f));
			obj.transform.localScale = new Vector3(1, 1.2f);
			
			// damage
			const int maxTargetEntity = 4;
			Entity.AttackAreaNoEffect(player, 1.1f, 0, plrPos+hitboxPos, hitboxSize, 0, maxTargetEntity);
			player.PlaySFX("swordSlash1");
			
			yield return new WaitForSeconds(attackLength2 * 0.94f/player.entityStat.attackSpeed);
		}
		
		private const float attackLength3 = 0.833f;
		private IEnumerator attack2Enumerator() {
			Vector2 mousePos = InputManager.inputData[InputActionType.MousePosition].valueV;
			bool    isBack   = mousePos.x < player.transform.position.x;
			
			player.animator.Play("Dattack3"+(isBack?"B":""), 0);
			
			Vector2 hitboxPos  = new Vector2(0.3f*(isBack?-1:1), -0.2f);
			Vector2 hitboxSize = new Vector2(1.7f,             0.4f);
			
			oldAnimatorSpeed      = player.animator.speed;
			player.animator.speed = player.entityStat.attackSpeed;
			
			yield return new WaitForSeconds(attackLength1 * 0.06f/player.entityStat.attackSpeed);
			
			GameObject obj = UObjectPool.instance.Get("SlashEffect", (Vector2)player.transform.position + new Vector2(0.3f*(isBack?-1:1)+Random.Range(-0.1f, 0.1f),-0.1f+Random.Range(-0.1f, 0.1f)));
			obj.transform.rotation   = Quaternion.Euler(0, 0, -85 + Random.Range(-2f, 2f));
			obj.transform.localScale = new Vector3(1.1f, 2f);
			
			// damage
			const int maxTargetEntity = 3;
			Entity.AttackAreaNoEffect(player, 1.5f, 0, plrPos+hitboxPos, hitboxSize, 0, maxTargetEntity);
			player.PlaySFX("swordSlash2");
			
			yield return new WaitForSeconds(attackLength2 * 0.94f/player.entityStat.attackSpeed);
		}

		private void OnAttackCancel() {
			player.animator.speed = oldAnimatorSpeed;
			player.animator.Play("idle");

			attackDelayedAction?.Cancel();
			
			//Debug.Log("Attack Cancelled");
		}

		private void OnAttackDone() {
			player.animator.speed = oldAnimatorSpeed;
			player.animator.Play("idle");
		}
		
		private ExclusiveAction attack1 => new (attack1Enumerator(), OnAttackCancel, OnAttackDone, 0, player);
		private ExclusiveAction attack2 => new (attack2Enumerator(), OnAttackCancel, OnAttackDone, 0, player);
		private ExclusiveAction attack3 => new (attack3Enumerator(), OnAttackCancel, OnAttackDone, 0, player);

		protected override ExclusiveAction GetAttackAction(int attackStage) {
			return attackStage switch {
				0 => attack1,
				1 => attack2,
				2 => attack3,
				_ => throw new ArgumentOutOfRangeException(nameof(attackStage), attackStage, null)
			};
		}

		protected override float           GetAttackDelay(int  attackStage) {
			return attackStage switch {
				0 => (attackLength1 /player.entityStat.attackSpeed) *.5f,
				1 => (attackLength2 /player.entityStat.attackSpeed) *.5f,
				2 => (attackLength3 /player.entityStat.attackSpeed) *.5f,
				_ => throw new ArgumentOutOfRangeException(nameof(attackStage), attackStage, null)
			};
		}

		protected override void InitializeAttacks() {
			attackStageCount = 3;
		}
	}
}