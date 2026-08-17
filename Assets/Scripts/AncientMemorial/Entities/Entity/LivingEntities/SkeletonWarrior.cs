using System.Collections;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Utility;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace AncientMemorial.Entities {
	public class SkeletonWarrior : Skeleton {
		
		// private const      float attackLength = 1.833f;
		// protected override float marginX => 1;
		//
		// protected override float moveTargetDistance   => 3;
		// protected override float moveAllowMargin      => 1;
		// protected override float moveTargetDistanceRM => 1;
		//
		// public override void OnStunStart() { }
		//
		// public override void OnStunEnd() { }
		//
		// protected override string footstepSoundName => "footstep"+Random.Range(1, 4);
		// protected override bool   isFootstep        => spriteRenderer.sprite.name is "SkeletonW_0" or "SkeletonW_4";
		//
		// private DelayedAction attackDelayedAction;
		// protected override IEnumerator AttackEnumerator() {
		// 	bool    isFlipped = (int)Mathf.Sign(player.transform.position.x - transform.position.x) == -1;
		// 	
		// 	rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
		// 	
		// 	oldAnimSpeed = animator.speed;
		// 	animator.speed  = entityStat.attackSpeed;
		// 	
		// 	attackAnimation = isFlipped ? attackF : attackB;
		// 	animator.SetBool(attackAnimation, true);
		// 	animator.SetBool(moving, false);
		// 	animator.SetTrigger(attacking);
		//
		// 	Vector2 hitboxPos  = (Vector2)transform.position + new Vector2(0.5395f * (isFlipped ? -1 : 1), -0.155f);
		// 	Vector2 hitboxSize = new(1.46f, 0.31f);
		// 	attackDelayedAction = AttackArea(1, (attackLength * 6f)/(entityStat.attackSpeed * 11f), hitboxPos, hitboxSize, 0, 1, false, true);
		// 	yield return new WaitForSeconds(attackLength * 6f/ (entityStat.attackSpeed * 11f));
		//
		// 	PlaySFX("swordSlash2");
		// 	
		// 	yield return new WaitForSeconds(attackLength * 5f/ (entityStat.attackSpeed * 11f));
		// 	
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attackAnimation, false);
		// 	animator.SetTrigger(attacking);
		// 	
		// 	yield return new WaitForSeconds((attackLength * 2)/(entityStat.attackSpeed * 11f));
		// }
		//
		// protected override void OnAttackDone() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attackAnimation, false);
		// 	animator.SetTrigger(attacking);
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		// protected override void OnAttackCancel() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attackAnimation, false);
		// 	animator.SetTrigger(attacking);
		//
		// 	attackDelayedAction.Cancel();
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		// private readonly float attackableDist = 0.6f;
		// private          float attackableDistRM = 0.1f;
		// private          float attackableDistRV;
		// public override void       AttackReadyRoutine() {
		// 	if (attackableDistRV == 0) {
		// 		attackableDistRV = Random.Range(-attackableDistRM, attackableDistRM);
		// 	}
		// 	
		// 	if (!player) {
		// 		state = EnemyState.Wander;
		// 		return;
		// 	}
		// 	
		// 	float targetPositionX = player.transform.position.x;
		// 	if (Mathf.Abs(targetPositionX - transform.position.x) <= attackableDist + attackableDistRV) {
		// 		state = EnemyState.Attack;
		// 		return;
		// 	}
		// 	
		// 	Move(targetPositionX);
		// }
		
		// protected override void Death() {
		// 	GameManager.UValueFloatVariables["skeletonWarriorDead"]   = new UPureFloat {number = GameManager.UValueFloatVariables["skeletonWarriorDead"].value + 1};
		// 	base.Death();
		// }
		
		protected override IEnumerator AttackEnumerator() {
			throw new System.NotImplementedException();
		}

		protected override void   OnAttackDone() {
			throw new System.NotImplementedException();
		}

		protected override void   OnAttackCancel() {
			throw new System.NotImplementedException();
		}

		protected override string footstepSoundName    { get; }
		protected override bool   isFootstep           { get; }
		protected override float  marginX              { get; }
		protected override float  moveTargetDistance   { get; }
		protected override float  moveAllowMargin      { get; }
		protected override float  moveTargetDistanceRM { get; }
	}
}