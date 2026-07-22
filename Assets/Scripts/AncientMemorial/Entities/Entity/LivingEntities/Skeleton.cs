using System;
using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Projectiles;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.ObjectPool;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public abstract class Skeleton : Enemy {
		protected static readonly int attackF  = Animator.StringToHash("attackFrontEnd");
		protected static readonly int attackB  = Animator.StringToHash("attackBackEnd");
		protected static readonly int attack   = Animator.StringToHash("attacking");
		protected static readonly int moving   = Animator.StringToHash("moving");
		protected static readonly int backward = Animator.StringToHash("backward");
		protected static readonly int Landing  = Animator.StringToHash("landing");
	
		[NonSerialized] private float oldAnimSpeed;
		[NonSerialized] private int   attackAnimation;

		public override void OnStunStart() { }

		public override void OnStunEnd() {
			currentExclusiveAction = FindingAggro;
		}
		
		[NonSerialized]private bool _FootstepSound = false;
		private bool FootstepSound {
			get => _FootstepSound;
			set {
				if (_FootstepSound == value) return;
				if (value) PlaySFX(footstepSoundName);
				_FootstepSound = value;
			}
		}

		protected abstract string footstepSoundName { get; }
		protected abstract bool isFootstep { get; }

		protected void Move(float targetPositionX) {
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
				
				// 발소리
				FootstepSound = isFootstep;
				
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

		[NonSerialized] private float         currWanderTargetPosX;
		[NonSerialized] private DelayedAction currWanderDA;
		public override void WanderRoutine() {
			if (currWanderDA is not { Executing: true }) {
				currWanderTargetPosX = transform.position.x;
				currWanderDA = new DelayedAction(Random.Range(5f, 20f), () => {
					currWanderTargetPosX = Random.Range(-MapManager.instance.GetMapSize().x / 2.2f,
														MapManager.instance.GetMapSize().x  / 2.2f);
				});
				currWanderDA.ExecuteDA();
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

		[NonSerialized] protected const float         moveTargetDistance   = 3f;
		[NonSerialized] protected const float         moveAllowMargin      = 1f;
		[NonSerialized] protected const float         moveTargetDistanceRM = 0.75f;
		[NonSerialized] protected       float         moveTargetDistanceRV;
		[NonSerialized] protected       DelayedAction currAlertDA;
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
				currAlertDA.ExecuteDA();
			}
			
			int signE = (int)Mathf.Sign(aggroEntity.transform.position.x - transform.position.x);
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
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).ExecuteDA();
			
			textAction.text = textSAction.text = new UPureString {Text = $"{Mathf.Floor(damage*100)/100f}"};
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
			
			if (attacker != player) return;
			if (entityStat.hp <= 0) {
				PlaySFX("skeletonDeath");
				
				GameManager.SetTimeScale(0.05f, 0.1f);
				CameraBrain.instance.ShakeLerp(1f*Mathf.Max(Mathf.Log(damage+3),0.5f), 10);
				CameraBrain.instance.ZoomLerp(-0.5f);
			}
			else {
				PlaySFX("skeletonExcited");
				
				GameManager.SetTimeScale(0.05f, 0.05f);
				CameraBrain.instance.ShakeLerp(0.4f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.25f);
			}
		}

		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			// STUN
			currentExclusiveAction = Stun(1);
			state                  = EnemyState.Stun;
			new DelayedAction(1, () => state = EnemyState.Alert).ExecuteDA();
			
			Vector2 pushDirection = pushDir??(attacker.transform.position - transform.position).normalized;
			float   velocity = damage;
			
			AddProcessToFixedUpdate(() => { rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse); });

			entityStat.hp -= damage;
		}

		protected override void OnHit(Projectile attacker, float damage, Vector2? pushDir) {
			// STUN
			currentExclusiveAction = Stun(1);
			state                  = EnemyState.Stun;
			new DelayedAction(1, () => state = EnemyState.Alert).ExecuteDA();
			
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
			GameManager.UValueFloatVariables["EnemyDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
			
			GameObject doogaegol = UObjectPool.instance.Get("doogaegol", (Vector2)transform.position +  new Vector2(-0.03125f, 0.21875f));
			Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
			
			doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
			doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);
			
			base.Death();
		}

		protected override void OnGrounded() {
			PlaySFX("skeletonAmbient");
			PlaySFX("skeletonExcited");
			
			currentExclusiveAction = Stun(0.5f);
			state                  = EnemyState.Stun;
			animator.SetBool(Landing, true);
			new DelayedAction(0.5f, () => {
				state = EnemyState.Alert;
				animator.SetBool(Landing, false);
			}).ExecuteDA();
		}
	}
}