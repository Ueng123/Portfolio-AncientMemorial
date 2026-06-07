using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AncientMemorial.Map;
using AncientMemorial.Projectiles;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace AncientMemorial.Entities {
	public class SkeletonWarrior : Enemy {
		private static readonly int attackF  = Animator.StringToHash("attackFrontEnd");
		private static readonly int attackB  = Animator.StringToHash("attackBackEnd");
		private static readonly int attack   = Animator.StringToHash("attacking");
		private static readonly int moving   = Animator.StringToHash("moving");
		private static readonly int backward = Animator.StringToHash("backward");

		public SpriteRenderer gumgiObject;

		private         float oldAnimSpeed;
		private         int   attackAnimation;
		private const   float attackLength = 0.9165f;

		public override void OnStunStart() { }

		public override void OnStunEnd() {
			
			currentExclusiveAction = FindingAggro;
		}

		protected override IEnumerator AttackEnumerator() {
			rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
			
			bool    isFlipped = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x) == -1;
			Vector2 gumgiPos  = new(0.393f * (isFlipped ? -1 : 1), -0.155f);
			Vector2 gumgiSize = new(0.66f, 0.09f);
			
			oldAnimSpeed = animator.speed;
			
			animator.speed  = entityStat.attackSpeed;
			attackAnimation = isFlipped ? attackF : attackB;
			
			animator.SetBool(attackAnimation, true);
			animator.SetTrigger(attack);
			
			yield return new WaitForSeconds((attackLength * 6f)/(entityStat.attackSpeed * 11f));

			Collider2D[] hitColliders = Physics2D.OverlapBoxAll(gumgiPos + (Vector2)transform.position, gumgiSize, 0f);
			foreach (Collider2D hit in hitColliders) {
				if (!hit.CompareTag("Entity")) continue;
				Entity entity = hit.GetComponent<Entity>();

				if (entity == this) continue;
				if (!aggroC1.Keys.ToList().Contains(entity.entityType)) continue;
				
				Debug.Log($"[HIT EVENT] SendingEvent : {entity}");
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, 10, new EntityHitData(
							  this,
							  null,
							  entity,
							  entityStat.attackDamage,
							  new Vector2(entity.transform.position.x - transform.position.x, 0).normalized
						  ));
			}
			
			yield return new WaitForSeconds((attackLength * 5)/(entityStat.attackSpeed * 11f));
		}

		protected override void OnAttackDone() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
		}

		protected override void OnAttackCancel() {
			animator.speed = oldAnimSpeed;
			animator.SetBool(attackAnimation, false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
		}

		private void Move(float targetPositionX) {
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

		private const float         moveTargetDistance   = 2.5f;
		private const float         moveAllowMargin      = 0.2f;
		private const float         moveTargetDistanceRM = 0.75f;
		private       float         moveTargetDistanceRV = 0f;
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
		
		private readonly float attackableDist = 1f;
		public override void       AttackReadyRoutine() {
			float targetPositionX = aggroEntity.transform.position.x;
			if (Mathf.Abs(targetPositionX - transform.position.x) <= attackableDist) state = EnemyState.Attack;
			
			Move(targetPositionX);
		}

		public override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			// STUN
			currentExclusiveAction = Stun(1);
			state                  = EnemyState.Stun;
			new DelayedAction(1, () => state = EnemyState.Alert).Execute();
			
			Vector2 pushDirection = pushDir??(attacker.transform.position - transform.position).normalized;
			float   velocity = damage;
			
			AddProcessToFixedUpdate(() => rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse));

			entityStat.hp -= damage;

			if (!(attacker == player && entityStat.hp <= 0)) return;
			Time.timeScale = 0.25f; 
			new DelayedAction(0.3f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
		}
		
		public override void OnHit(Projectile attacker, float damage, Vector2? pushDir) {
			// STUN
			currentExclusiveAction = Stun(1);
			state                  = EnemyState.Stun;
			new DelayedAction(1, () => state = EnemyState.Alert).Execute();
			
			Vector2 pushDirection = pushDir??new Vector2(transform.position.x-attacker.transform.position.x, 0).normalized;
			float   velocity      = damage*2;
			
			AddProcessToFixedUpdate(() => rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse));
			
			entityStat.hp -= damage;

			if (!(attacker.owner == player && entityStat.hp <= 0)) return;
			Time.timeScale = 0.25f; 
			new DelayedAction(0.3f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
		}
		
		public override void OnHit(float damage, Vector2? pushDir) {
			if (pushDir.HasValue) {
				float velocity = damage * 2;
				AddProcessToFixedUpdate(() => rigidbody2D.AddForce(pushDir.Value * velocity, ForceMode2D.Impulse));
			}
			
			entityStat.hp -= damage;
		}

		protected override void Death() {
			GameObject doogaegol = UObjectPool.instance.Get("doogaegol", (Vector2)transform.position +  new Vector2(-0.03125f, 0.21875f));
			Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
			
			doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
			doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);

			base.Death();
		}
	}
}