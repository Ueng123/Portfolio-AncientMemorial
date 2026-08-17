using System.Collections;
using AncientMemorial.Projectiles;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class SkeletonArcher : Skeleton {
		// private const      float attackLength = 1.833f;
		// protected override float marginX            => 1;
		//
		// protected override float moveTargetDistance   => 5;
		// protected override float moveAllowMargin      => 1;
		// protected override float moveTargetDistanceRM => 1;
		//
		// public override void OnStunStart() { }
		//
		// public override void OnStunEnd() { }
		//
		// protected override string footstepSoundName => "footstep"+Random.Range(1, 4);
		// protected override bool   isFootstep        => spriteRenderer.sprite.name is "SkeletonA_0" or "SkeletonA_4";
		//
		// private float v0;
		//
		// private void SetRandomVelocity() {
		// 	v0 = Random.Range(9f, 11f);
		// }
		//
		// private Vector2 GetArrowVelocity() {
		// 	float g  = Mathf.Abs(Physics2D.gravity.y);
		// 	float x1 = player.transform.position.x;
		// 	float y1 = player.transform.position.y;
		// 	float x0 = transform.position.x;
		// 	float y0 = transform.position.y;
		// 	float xl = x1 - x0;
		// 	float yl = y1 - y0;
		// 	float t = Mathf.Atan2(
		// 		xl * xl * g,
		// 		xl * v0 * v0 - Mathf.Sign(xl)*Mathf.Sqrt(
		// 			xl*xl*v0*v0*v0*v0 - xl*xl*xl*xl*g*g - 2*xl*xl*yl*g*v0*v0)
		// 		);
		// 	return new Vector2(v0*Mathf.Cos(t), v0*Mathf.Sin(t));
		// }
		//
		// private bool ArrowShootable() {
		// 	float g  = Mathf.Abs(Physics2D.gravity.y);
		// 	float x1 = player.transform.position.x;
		// 	float y1 = player.transform.position.y;
		// 	float x0 = transform.position.x;
		// 	float y0 = transform.position.y;
		// 	float xl = x1 - x0;
		// 	float yl = y1 - y0;
		// 	
		// 	return xl*xl*v0*v0*v0*v0 - xl*xl*xl*xl*g*g - 2*xl*xl*yl*g*v0*v0 >= 0;
		// }
		//
		// protected override IEnumerator AttackEnumerator() {
		// 	rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
		// 	Vector2 velocityAngle = GetArrowVelocity();
		// 	
		// 	shootable    = false;
		// 	oldAnimSpeed = animator.speed;
		// 	
		// 	animator.speed  = entityStat.attackSpeed;
		// 	animator.SetBool(moving, false);
		// 	animator.SetTrigger(attacking);
		// 	
		// 	yield return new WaitForSeconds((attackLength * 6f)/(entityStat.attackSpeed * 11f));
		// 	
		// 	// 화살발싸
		// 	GameObject arrowObject = UObjectPool.instance.Get("Arrow", transform.position);
		// 	Arrow arrow = arrowObject.GetComponent<Arrow>();
		//
		// 	arrow.rigidbody2D.linearVelocity = velocityAngle;
		// 	arrow.damage                     = entityData.attackDamage;
		// 	arrow.owner                      = this;
		//
		// 	PlaySFX("bowShoot");
		// 	
		// 	yield return new WaitForSeconds((attackLength * 5)/(entityStat.attackSpeed * 11f));
		// 	
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attacking, true);
		// 	
		// 	yield return new WaitForSeconds((attackLength * 2)/(entityStat.attackSpeed * 11f));
		// }
		//
		// protected override void OnAttackDone() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attacking, false);
		// 	animator.SetTrigger(attack);
		// 	
		// 	SetRandomVelocity();
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		// protected override void OnAttackCancel() {
		// 	animator.speed = oldAnimSpeed;
		// 	animator.SetBool(attacking, false);
		// 	
		// 	SetRandomVelocity();
		// 	
		// 	state                  = EnemyState.Alert;
		// }
		//
		//
		//
		// public override void Initialize() {
		// 	base.Initialize();
		// 	SetRandomVelocity();
		// }
		//
		// private bool shootable = false;
		// public override void AttackReadyRoutine() {
		// 	if (!player) {
		// 		state = EnemyState.Wander;
		// 		return;
		// 	}
		// 	
		// 	if (attackable && shootable) {
		// 		state = EnemyState.Attack;
		// 		return;
		// 	}
		// 	
		// 	if (currAlertDA is not { Executing: true }) {
		// 		currAlertDA = new DelayedAction(Random.Range(1f, 3f), () => {
		// 			moveTargetDistanceRV = Random.Range(-moveTargetDistanceRM, moveTargetDistanceRM);
		// 		});
		// 		currAlertDA.ExecuteDA();
		// 	}
		// 	
		// 	int   signE           = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
		// 	float targetPositionX = player.transform.position.x + moveTargetDistanceRV - moveTargetDistance*signE;
		// 	
		// 	Move(targetPositionX);
		// }
		//
		// protected override void FixedRoutine() {
		// 	base.FixedRoutine();
		//
		// 	if (state == EnemyState.AttackReady) shootable = ArrowShootable();
		// }
		//
		// protected override void Death() {
		// 	GameManager.UValueFloatVariables["skeletonArcherDead"]   = new UPureFloat {number = GameManager.UValueFloatVariables["skeletonWarriorDead"].value + 1};
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