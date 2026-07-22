using System.Collections;
using System.Linq;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using JetBrains.Annotations;
using UengSystem.Events;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Vector2 = UnityEngine.Vector2;

namespace AncientMemorial.Entities {
	public class SkeletonWarrior : Skeleton {

		private         float oldAnimSpeed;
		private         int   attackAnimation;
		private const   float attackLength = 1.833f;

		public override void OnStunStart() { }

		public override void OnStunEnd() {
			currentExclusiveAction = FindingAggro;
		}

		protected override string footstepSoundName => "footstep"+Random.Range(1, 4);
		protected override bool   isFootstep        => spriteRenderer.sprite.name is "SkeletonW_0" or "SkeletonW_4";
		
		private DelayedAction attackDelayedAction;
		protected override IEnumerator AttackEnumerator() {
			bool    isFlipped = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x) == -1;
			
			rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
			
			oldAnimSpeed = animator.speed;
			animator.speed  = entityStat.attackSpeed;
			
			attackAnimation = isFlipped ? attackF : attackB;
			animator.SetBool(attackAnimation, true);
			animator.SetBool(moving, false);
			animator.SetTrigger(attack);

			Vector2 hitboxPos  = (Vector2)transform.position + new Vector2(0.393f * (isFlipped ? -1 : 1), -0.155f);
			Vector2 hitboxSize = new(0.88f, 0.31f);
			attackDelayedAction = AttackArea(1, (attackLength * 6f)/(entityStat.attackSpeed * 11f), hitboxPos, hitboxSize, 0, 1, false, true);
			yield return new WaitForSeconds(attackLength * 6f/ (entityStat.attackSpeed * 11f));

			PlaySFX("swordSlash2");
			
			yield return new WaitForSeconds(attackLength * 5f/ (entityStat.attackSpeed * 11f));
			
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			animator.SetTrigger(attack);
			
			yield return new WaitForSeconds((attackLength * 2)/(entityStat.attackSpeed * 11f));
		}

		protected override void OnAttackDone() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			animator.SetTrigger(attack);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
		}

		protected override void OnAttackCancel() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			animator.SetTrigger(attack);

			attackDelayedAction.Cancel();
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
		}
		
		private readonly float attackableDist = 0.9f;
		private          float attackableDistRM = 0.2f;
		private          float attackableDistRV;
		public override void       AttackReadyRoutine() {
			if (attackableDistRV == 0) {
				attackableDistRV = Random.Range(-attackableDistRM, attackableDistRM);
			}
			
			if (!aggroEntity) {
				state = EnemyState.Wander;
				return;
			}
			
			float targetPositionX = aggroEntity.transform.position.x;
			if (Mathf.Abs(targetPositionX - transform.position.x) <= attackableDist + attackableDistRV) {
				state = EnemyState.Attack;
				return;
			}
			
			Move(targetPositionX);
		}
		
		protected override void Death() {
			GameManager.UValueFloatVariables["skeletonWarriorDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["skeletonWarriorDead"].value + 1};
			base.Death();
		}
	}
}