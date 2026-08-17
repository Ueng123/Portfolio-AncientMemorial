using AncientMemorial.Cameras;
using AncientMemorial.Map;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public abstract class Skeleton : Enemy {
		protected static readonly int attackF   = Animator.StringToHash("attackFrontEnd");
		protected static readonly int attackB   = Animator.StringToHash("attackBackEnd");
		protected static readonly int attack    = Animator.StringToHash("attack");
		protected static readonly int attacking = Animator.StringToHash("attacking");
		protected static readonly int moving    = Animator.StringToHash("moving");
		protected static readonly int backward  = Animator.StringToHash("backward");
		protected static readonly int Landing   = Animator.StringToHash("landing");
	
		protected float oldAnimSpeed;
		protected int   attackAnimation;

		public override void OnStunStart() { }

		public override void OnStunEnd() { }
		
		private bool _FootstepSound = false;
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
		protected abstract float marginX   { get; }

		protected void Move(float targetPositionX) {
			animator.SetBool(Falling, !isGround);
			
			targetPositionX = Mathf.Clamp(targetPositionX, MapManager.leftWall + marginX, MapManager.rightWall - marginX);
			
			if (!isGround) return;
			if (player) {
				// (+) : 이 엔티티가 타겟엔티티보다 <-에 있음
				int signE = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
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

		private float         currWanderTargetPosX;
		private DelayedAction currWanderDA;
		private float         newWanderOffsetMin => Mathf.Clamp01((transform.position.x - MapManager.leftWall) / 2f); // 왼쪽 벽으로부터 얼마나 떨어져있는지
		private float         newWanderOffsetMax => Mathf.Clamp01((MapManager.rightWall - transform.position.x) / 2f); // 오른쪽 벽으로부터 얼마나 떨어져있는지
		
		// public override void WanderRoutine() {
		// 	if (Mathf.Approximately(currWanderTargetPosX, default)) currWanderTargetPosX = transform.position.x;
		// 	
		// 	if (currWanderDA is not { Executing: true }) {
		// 		
		// 		currWanderDA = new DelayedAction(currWanderDA==null?Random.Range(0.5f, 1.5f):Random.Range(3f, 7f),
		// 										 () => {
		// 											 currWanderTargetPosX 
		// 												 = transform.position.x + Random.Range(-5*newWanderOffsetMin, 5*newWanderOffsetMax);
		// 										 });
		// 		currWanderDA.ExecuteDA();
		// 	}
		//
		// 	Move(currWanderTargetPosX);
		// 	
		// 	if (!player) return;
		// 	currWanderDA.Cancel();
		// 	// state = EnemyState.Alert;
		// }
		
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

		protected abstract float         moveTargetDistance   { get; }
		protected abstract float         moveAllowMargin      { get; }
		protected abstract float         moveTargetDistanceRM { get; }
		protected          float         moveTargetDistanceRV;
		protected          DelayedAction currAlertDA;
		// public override void AlertRoutine() {
		// 	// if (!player) {
		// 	// 	state = EnemyState.Wander;
		// 	// 	return;
		// 	// }
		// 	//
		// 	// if (attackable) {
		// 	// 	state = EnemyState.AttackReady;
		// 	// 	return;
		// 	// }
		//
		// 	if (currAlertDA is not { Executing: true }) {
		// 		currAlertDA = new DelayedAction(Random.Range(1f, 3f), () => {
		// 			moveTargetDistanceRV = Random.Range(-moveTargetDistanceRM, moveTargetDistanceRM);
		// 		});
		// 		currAlertDA.ExecuteDA();
		// 	}
		// 	
		// 	int signE = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
		// 	float targetPositionX = player.transform.position.x + moveTargetDistanceRV - moveTargetDistance*signE;
		// 	
		// 	Move(targetPositionX);
		// }

		// public override void AttackRoutine() { }

		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			ShowDamageUI(damage);
			
			// ((UObject)this).state = Stun(1);
			// state                  = EnemyState.Stun;
			// new DelayedAction(1, () => state = EnemyState.Alert).ExecuteDA();
			//
			Vector2 pushDirection = pushDir??(attacker.transform.position - transform.position).normalized;
			float   velocity      = 0.5f + Mathf.Log(damage, 500);
			
			AddProcessToFixedUpdate(() => { rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse); });
			
			if (entityStat.hp <= 0) {
				PlaySFX("skeletonDeath");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraBrain.instance.ShakeLerp(2f, 10);
				CameraBrain.instance.ZoomLerp(-0.2f);
			}
			else {
				PlaySFX("skeletonExcited");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraBrain.instance.ShakeLerp(1f, 10f);
				CameraBrain.instance.ZoomLerp(-0.1f);
			}
		}

		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}

		protected override void Death() {
			GameObject doogaegol = UObjectPool.instance.Get("doogaegol", (Vector2)transform.position +  new Vector2(-0.03125f, 0.21875f));
			Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
			
			doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
			doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);
			
			base.Death();
		}

		protected override void OnGrounded() {
			PlaySFX("skeletonLand");
			
			// ((UObject)this).state = Stun(0.5f);
			// state                  = EnemyState.Stun;
			
			// new DelayedAction(0.5f, () => {
			// 	state = EnemyState.Alert;
			// 	animator.SetBool(Landing, false);
			// }).ExecuteDA();
			
			animator.SetBool(Landing, true);
		}
	}
}