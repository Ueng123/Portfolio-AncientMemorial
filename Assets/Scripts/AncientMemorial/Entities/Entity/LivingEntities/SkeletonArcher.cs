using System.Collections;
using System.Linq;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Projectiles;
using UengSystem.Events;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class SkeletonArcher : Skeleton {
		private static readonly int attackF  = Animator.StringToHash("attackFrontEnd");
		private static readonly int attackB  = Animator.StringToHash("attackBackEnd");
		private static readonly int attacking   = Animator.StringToHash("attacking");
		private static readonly int attack   = Animator.StringToHash("attack");
		private static readonly int moving   = Animator.StringToHash("moving");
		private static readonly int backward = Animator.StringToHash("backward");
		private static readonly int Landing  = Animator.StringToHash("landing");

		private         float oldAnimSpeed;
		private const   float attackLength = 1.833f;

		public override void OnStunStart() { }

		public override void OnStunEnd() {
			currentExclusiveAction = FindingAggro;
		}

		protected override string footstepSoundName => "footstep"+Random.Range(1, 4);
		protected override bool   isFootstep        => spriteRenderer.sprite.name is "SkeletonA_0" or "SkeletonA_4";

		private const float v0 = 10f;
		private Vector2 GetArrowVelocity() {
			float g  = Mathf.Abs(Physics2D.gravity.y);
			float x1 = aggroEntity.transform.position.x;
			float y1 = aggroEntity.transform.position.y;
			float x0 = transform.position.x;
			float y0 = transform.position.y;
			float xl = x1 - x0;
			float yl = y1 - y0;
			float t = Mathf.Atan2(
				xl * xl * g,
				xl * v0 * v0 - Mathf.Sign(xl)*Mathf.Sqrt(
					xl*xl*v0*v0*v0*v0 - xl*xl*xl*xl*g*g - 2*xl*xl*yl*g*v0*v0)
				);
			return new Vector2(v0*Mathf.Cos(t), v0*Mathf.Sin(t));
		}

		private bool ArrowShootable() {
			float g  = Mathf.Abs(Physics2D.gravity.y);
			float x1 = aggroEntity.transform.position.x;
			float y1 = aggroEntity.transform.position.y;
			float x0 = transform.position.x;
			float y0 = transform.position.y;
			float xl = x1 - x0;
			float yl = y1 - y0;
			
			return xl*xl*v0*v0*v0*v0 - xl*xl*xl*xl*g*g - 2*xl*xl*yl*g*v0*v0 >= 0;
		}
		
		protected override IEnumerator AttackEnumerator() {
			rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
			Vector2 velocityAngle = GetArrowVelocity();
			
			shootable    = false;
			oldAnimSpeed = animator.speed;
			
			animator.speed  = entityStat.attackSpeed;
			animator.SetBool(moving, false);
			animator.SetTrigger(attacking);
			
			yield return new WaitForSeconds((attackLength * 6f)/(entityStat.attackSpeed * 11f));
			
			// 화살발싸
			GameObject arrowObject = UObjectPool.instance.Get("Arrow", transform.position);
			Arrow arrow = arrowObject.GetComponent<Arrow>();

			arrow.rigidbody2D.linearVelocity = velocityAngle;
			arrow.damage                     = entityData.attackDamage;
			arrow.owner                      = this;
			
			yield return new WaitForSeconds((attackLength * 5)/(entityStat.attackSpeed * 11f));
			
			animator.speed = oldAnimSpeed;
			animator.SetBool(attacking, true);
			
			yield return new WaitForSeconds((attackLength * 2)/(entityStat.attackSpeed * 11f));
		}

		protected override void OnAttackDone() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attacking, false);
			animator.SetTrigger(attack);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
		}

		protected override void OnAttackCancel() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attacking, false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
		}
		
		private          bool      shootable               = false;
		private const    float     shootableCheckTime      = 0.1f;
		private readonly StopWatch shootableCheckStopwatch = new ();
		public override void       AttackReadyRoutine() {
			if (!aggroEntity) {
				state = EnemyState.Wander;
				return;
			}
			
			if (!shootableCheckStopwatch.Check(shootableCheckTime)) {
				shootableCheckStopwatch.Tick();
				shootable = ArrowShootable();
			}
			
			if (attackable && shootable) {
				state = EnemyState.Attack;
				return;
			}
			
			if (currAlertDA is not { Executing: true }) {
				currAlertDA = new DelayedAction(Random.Range(1f, 3f), () => {
					moveTargetDistanceRV = Random.Range(-moveTargetDistanceRM, moveTargetDistanceRM);
				});
				currAlertDA.ExecuteDA();
			}
			
			int   signE           = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x);
			float targetPositionX = aggroEntity.transform.position.x + moveTargetDistanceRV - moveTargetDistance*signE;
			
			Move(targetPositionX);
		}
		
		protected override void Death() {
			GameManager.UValueFloatVariables["skeletonArcherDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["skeletonWarriorDead"].value + 1};
			base.Death();
		}
	}
}