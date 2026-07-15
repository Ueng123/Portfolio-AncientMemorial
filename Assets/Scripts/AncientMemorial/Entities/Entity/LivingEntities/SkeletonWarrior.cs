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
	public class SkeletonWarrior : Enemy {
		private static readonly int attackF  = Animator.StringToHash("attackFrontEnd");
		private static readonly int attackB  = Animator.StringToHash("attackBackEnd");
		private static readonly int attack   = Animator.StringToHash("attacking");
		private static readonly int moving   = Animator.StringToHash("moving");
		private static readonly int backward = Animator.StringToHash("backward");
		private static readonly int Landing  = Animator.StringToHash("landing");

		public SpriteRenderer gumgiObject;

		private         float oldAnimSpeed;
		private         int   attackAnimation;
		private const   float attackLength = 1.833f;

		public override void OnStunStart() { }

		public override void OnStunEnd() {
			currentExclusiveAction = FindingAggro;
		}

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
			yield return new WaitForSeconds(attackLength/entityStat.attackSpeed);
			
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

		private int mi = 0;
		private void Move(float targetPositionX) {
			animator.SetBool(Falling, !isGround);
			
			if (!isGround) return;
			
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
			
				if (movingB) animator.speed = entityStat.moveSpeed * (backwardB ? 1.5f : 1);
				else animator.speed         = 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
				
				rigidbody2D.linearVelocityX = entityStat.moveSpeed * signT;
				// Debug.Log($"[Entity Velocity] HE IS MOVING {mi++}");
			}
			else {
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				bool movingB   = Mathf.Abs(targetPositionX - transform.position.x) > moveAllowMargin;
			
				// 애니메이션
				spriteRenderer.flipX = signT == 1;
			
				animator.SetBool(moving,   movingB);
				animator.SetBool(backward, false);
			
				if (movingB) animator.speed = entityStat.moveSpeed;
				else animator.speed         = 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
				
				rigidbody2D.linearVelocityX = entityStat.moveSpeed * signT;
				// Debug.Log($"[Entity Velocity] HE IS MOVING {mi++}");
			}
		}

		private float         currWanderTargetPosX;
		private DelayedAction currWanderDA;
		public override void WanderRoutine() {
			if (currWanderDA is not { Executing: true }) {
				currWanderDA = new DelayedAction(Random.Range(5f, 20f), () => {
					currWanderTargetPosX = Random.Range(-MapManager.instance.GetMapSize().x / 2.2f,
														MapManager.instance.GetMapSize().x  / 2.2f);
				});
				currWanderDA.Execute();
			}

			Move(currWanderTargetPosX);
			
			if (!aggroEntity) return;
			currWanderDA.Cancel();
			state = EnemyState.Alert;
		}
		
		// private readonly StopWatch _velocityRefresh = new ();
		// private          float     _velocityScale   = -1;
		// private float GetVelocityScale(float targetPositionX) {
		// 	if (_velocityScale >= 0 && _velocityRefresh.Check(0.1f)) return _velocityScale;
		// 	
		// 	float vM = entityStat.moveSpeed;
		// 	float vm = entityStat.moveSpeed / 2;
		// 	float x  = transform.position.x;
		//
		// 	float w = -Mathf.Pow(Mathf.Abs((x - targetPositionX)), 3);
		// 	_velocityScale = vm - (vM - vm) * (Mathf.Exp(w) - 1);
		// 	_velocityRefresh.Tick();
		// 	
		// 	return _velocityScale;
		// }

		private const float         moveTargetDistance   = 3f;
		private const float         moveAllowMargin      = 1f;
		private const float         moveTargetDistanceRM = 0.75f;
		private       float         moveTargetDistanceRV;
		private       DelayedAction currAlertDA;
		public override void AlertRoutine() {
			if (!aggroEntity) {
				state = EnemyState.Wander;
				return;
			}
			
			if (attackable) {
				state = EnemyState.AttackReady;
				attackableDistRV = Random.Range(-attackableDistRM, attackableDistRM);
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
		
		private readonly float attackableDist = 0.9f;
		private          float attackableDistRM = 0.2f;
		private          float attackableDistRV;
		public override void       AttackReadyRoutine() {
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
				CameraBrain.instance.ShakeLerp(1f*Mathf.Max(Mathf.Log(damage+3),0.5f), 10);
				CameraBrain.instance.ZoomLerp(-0.5f);
			}
			else {
				Time.timeScale = 0.05f;
				new DelayedAction(0.05f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
				CameraBrain.instance.ShakeLerp(0.4f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
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
			
			AddProcessToFixedUpdate(() => { rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse); });

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
			
			GameManager.UValueFloatVariables["skeletonWarriorDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["skeletonWarriorDead"].value + 1};
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