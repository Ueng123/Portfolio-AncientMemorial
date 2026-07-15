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
	public class SkeletonArcher : Enemy {
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

		private void Move(float targetPositionX) {
			animator.SetBool(Falling, !isGround);
			
			if (!isGround) return;
			
			Debug.Log("HE IS MOVING");
			if (aggroEntity) {
				// (+) : 이 엔티티가 타겟엔티티보다 <-에 있음
				int signE = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x);
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				bool movingB   = Mathf.Abs(targetPositionX - transform.position.x) > moveAllowMargin;
				bool backwardB = signT * signE                                     == -1;
			
				// 애니메이션
				spriteRenderer.flipX = signE == 1;
			
				animator.SetBool(moving,   movingB);
				animator.SetBool(backward, backwardB);
			
				if (movingB) animator.speed = GetVelocityScale(targetPositionX) * (backwardB ? 1.5f : 1);
				else animator.speed         = 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
			
				Vector2 velocity = new (GetVelocityScale(targetPositionX) * signT, rigidbody2D.linearVelocity.y);
				rigidbody2D.linearVelocity = velocity;
			}
			else {
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				bool movingB   = Mathf.Abs(targetPositionX - transform.position.x) > moveAllowMargin;
			
				// 애니메이션
				spriteRenderer.flipX = signT == 1;
			
				animator.SetBool(moving,   movingB);
				animator.SetBool(backward, false);
			
				if (movingB) animator.speed = GetVelocityScale(targetPositionX);
				else animator.speed         = 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
			
				Vector2 velocity = new (GetVelocityScale(targetPositionX) * signT, rigidbody2D.linearVelocity.y);
				rigidbody2D.linearVelocity = velocity;
			}
		}

		private float     currWanderTargetPosX;
		private float     wanderTime;
		public override void WanderRoutine() {
			if (!aggroEntity) return;
			state = EnemyState.Alert;
		}
		
		private readonly StopWatch _velocityRefresh = new ();
		private          float     _velocityScale   = -1;
		private float GetVelocityScale(float targetPositionX) {
			if (_velocityScale >= 0 && _velocityRefresh.Check(0.1f)) return _velocityScale;
			
			float vM = entityStat.moveSpeed;
			float vm = entityStat.moveSpeed / 2;
			float x  = transform.position.x;

			float w = -Mathf.Pow(Mathf.Abs((x - targetPositionX)), 3);
			_velocityScale = vm - (vM - vm) * (Mathf.Exp(w) - 1);
			_velocityRefresh.Tick();
			
			return _velocityScale;
		}

		private const float         moveTargetDistance   = 7f;
		private const float         moveAllowMargin      = 1f;
		private const float         moveTargetDistanceRM = 4f;
		private       float         moveTargetDistanceRV;
		private       DelayedAction currAlertDA;
		public override void AlertRoutine() {
			if (!aggroEntity) {
				state = EnemyState.Wander;
				return;
			}
			
			if (attackable) {
				state = EnemyState.AttackReady;
				return;
			}

			if (currAlertDA is not { Executing: true }) {
				currAlertDA = new DelayedAction(Random.Range(1f, 3f), () => {
					moveTargetDistanceRV = Random.Range(-moveTargetDistanceRM, moveTargetDistanceRM);
				});
				currAlertDA.Execute();
			}
			
			int signE = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x);
			float targetPositionX = aggroEntity.transform.position.x + moveTargetDistanceRV - moveTargetDistance*signE;
			
			Move(targetPositionX);
		}
		
		private          float     attackableDistRV;
		private          bool      shootable = false;
		private const    float     shootableCheckTime      = 0.1f;
		private          StopWatch shootableCheckStopwatch = new ();
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
				currAlertDA.Execute();
			}
			
			int   signE           = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x);
			float targetPositionX = aggroEntity.transform.position.x + moveTargetDistanceRV - moveTargetDistance*signE;
			
			Move(targetPositionX);
		}

		public override void AttackRoutine() { }

		public override void StunRoutine() { }

		public override void HitEffect(Entity attacker, float damage, Vector2? pushDir = null) {
			UUI damageUI = UUIObjectPool.instance.Open("DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition =
				(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction textAction  = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).Execute();
			
			textAction.text = textSAction.text = new UPureString {Text = $"{Mathf.Floor(damage*100)/100f}"};
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
			
			if (attacker != player) return;
			if (entityStat.hp <= 0) {
				Time.timeScale = 0.05f;
				new DelayedAction(0.1f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				CameraBrain.instance.ShakeLerp(1f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.5f);
			}
			else {
				Time.timeScale = 0.05f;
				new DelayedAction(0.05f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				CameraBrain.instance.ShakeLerp(0.4f*Mathf.Max(Mathf.Log(damage+3),0.5f), 10);
				CameraBrain.instance.ZoomLerp(-0.25f);
			}
		}

		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			// STUN
			currentExclusiveAction = Stun(1);
			state                  = EnemyState.Stun;
			new DelayedAction(1, () => state = EnemyState.Alert).Execute();
			
			Vector2 pushDirection = pushDir??(attacker.transform.position - transform.position).normalized;
			float   velocity = damage;
			
			AddProcessToFixedUpdate(() => rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse));

			entityStat.hp -= damage;
		}

		protected override void OnHit(Projectile attacker, float damage, Vector2? pushDir) {
			// STUN
			currentExclusiveAction = Stun(1);
			state                  = EnemyState.Stun;
			new DelayedAction(1, () => state = EnemyState.Alert).Execute();
			
			Vector2 pushDirection = pushDir??new Vector2(transform.position.x-attacker.transform.position.x, 0).normalized;
			float   velocity      = damage*2;
			
			AddProcessToFixedUpdate(() => rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse));
			
			entityStat.hp -= damage;
		}

		protected override void OnHit(float damage, Vector2? pushDir) {
			if (pushDir.HasValue) {
				float velocity = damage * 2;
				AddProcessToFixedUpdate(() => rigidbody2D.AddForce(pushDir.Value * velocity, ForceMode2D.Impulse));
			}
			
			entityStat.hp -= damage;
		}

		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}

		protected override void Death() {
			GameObject doogaegol = UObjectPool.instance.Get("doogaegol", (Vector2)transform.position +  new Vector2(-0.03125f, 0.21875f));
			Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
			
			doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
			doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);
			
			GameManager.UValueFloatVariables["skeletonArcherDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["skeletonArcherDead"].value + 1};
			GameManager.UValueFloatVariables["EnemyDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
			
			base.Death();
		}

		protected override void OnGrounded() {
			currentExclusiveAction = Stun(0.5f);
			state                  = EnemyState.Stun;
			animator.SetBool(Landing, true);
			new DelayedAction(0.5f, () => {
				state = EnemyState.Alert;
				animator.SetBool(Landing, false);
			}).Execute();
		}
	}
}