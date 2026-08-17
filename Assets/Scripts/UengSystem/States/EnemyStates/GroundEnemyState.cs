using AncientMemorial.Map;
using UnityEngine;

namespace UengSystem.States.EnemyStates {
	public abstract class GroundEnemyState : EnemyState {
		private static readonly int moving   = Animator.StringToHash("moving");
		private static readonly int backward = Animator.StringToHash("backward");
		
		private bool _FootstepSound = false;
		private bool FootstepSound {
			get => _FootstepSound;
			set {
				if (_FootstepSound == value) return;
				if (value) enemy.PlaySFX(footstepSoundName);
				_FootstepSound = value;
			}
		}

		protected abstract string footstepSoundName { get; }
		protected abstract bool   isFootstep        { get; }

		private bool _isMoving = false;
		private bool _movingBackward = false;
		
		protected bool isMoving {
			get => _isMoving;
			set {
				if (_isMoving == value) return;
				_isMoving = value;
				
				enemy.animator.SetBool(moving, value);
			}
		}

		protected bool movingBackward {
			get => _movingBackward;
			set {
				if (_movingBackward == value) return;
				_movingBackward = value;
				
				enemy.animator.SetBool(backward, value);
			}
		}

		protected void Move(float targetPositionX, float mapMargin, float moveMargin) {
			targetPositionX = Mathf.Clamp(targetPositionX, MapManager.leftWall + mapMargin, MapManager.rightWall - mapMargin);
			
			Transform      transform      = enemy.transform;
			SpriteRenderer spriteRenderer = enemy.spriteRenderer;
			Rigidbody2D    rigidbody2D    = enemy.rigidbody2D;
			Animator       animator       = enemy.animator;
			
			if (!enemy.isGround) return;
			if (target) {
				// (+) : 이 엔티티가 타겟엔티티보다 <-에 있음
				int signE = (int)Mathf.Sign(target.transform.position.x - transform.position.x);
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				isMoving    = Mathf.Abs(targetPositionX - transform.position.x) > moveMargin;
				movingBackward = signT * signE                                     == -1;
			
				// 애니메이션
				spriteRenderer.flipX = signE == 1;
			
				if (isMoving) animator.speed = enemy.entityStat.moveSpeed * (movingBackward ? 1.5f : 1);
				else animator.speed          = 1;
			
				// 안움직일 경우 거르기
				if (!isMoving) return;
				
				// 발소리
				FootstepSound = isFootstep;
				
				rigidbody2D.linearVelocityX = enemy.entityStat.moveSpeed * signT * (movingBackward ? 0.3f : 1);
				// Debug.Log($"[Entity Velocity] HE IS MOVING {mi++}");
			}
			else {
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				bool movingB   = Mathf.Abs(targetPositionX - transform.position.x) > moveMargin;
			
				// 애니메이션
				spriteRenderer.flipX = signT == 1;
			
				if (movingB) animator.speed = enemy.entityStat.moveSpeed;
				else animator.speed         = 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
				
				rigidbody2D.linearVelocityX = enemy.entityStat.moveSpeed * signT;
				// Debug.Log($"[Entity Velocity] HE IS MOVING {mi++}");
			}
		}
	}
}