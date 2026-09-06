using AncientMemorial.Entities;
using AncientMemorial.Map;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates {
	public abstract class GroundEnemyState : EnemyState {
		private static readonly int moving   = Animator.StringToHash("moving");
		private static readonly int backward = Animator.StringToHash("backward");
		
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
			Entity target = stateMachine.getTarget.Invoke();
			targetPositionX = Mathf.Clamp(targetPositionX, MapManager.leftWall + mapMargin, MapManager.rightWall - mapMargin);
			
			Transform      transform      = enemy.transform;
			SpriteRenderer spriteRenderer = enemy.spriteRenderer;
			Rigidbody2D    rigidbody2D    = enemy.rigidbody2D;
			
			if (!enemy.isGround) return;
			if (target) {
				// (+) : 이 엔티티가 타겟엔티티보다 <-에 있음
				int signE = (int)Mathf.Sign(target.transform.position.x - transform.position.x);
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
			
				// 애니메이션
				isMoving = Mathf.Abs(targetPositionX - transform.position.x) > moveMargin;
				movingBackward = signT != signE;
				
				spriteRenderer.flipX = signE == 1;
			
				// 안움직일 경우 거르기
				bool willMove    = Mathf.Abs(targetPositionX - transform.position.x) > moveMargin;
				if (!willMove) return;
				
				rigidbody2D.linearVelocityX = enemy.stat.moveSpeed * signT * (movingBackward ? 0.5f : 1f);
				// Debug.Log($"[Entity Velocity] HE IS MOVING {mi++}");
			}
			else {
				// (+) : 이 엔티티가 타켓위치보다 <-에 있음
				int signT = (int)Mathf.Sign(targetPositionX - transform.position.x);
				bool movingB   = Mathf.Abs(targetPositionX - transform.position.x) > moveMargin;
			
				// 애니메이션
				spriteRenderer.flipX = signT == 1;
			
				// 안움직일 경우 거르기
				if (!movingB) return;
				
				rigidbody2D.linearVelocityX = enemy.stat.moveSpeed * signT;
				// Debug.Log($"[Entity Velocity] HE IS MOVING {mi++}");
			}
		}

		public override void OnFixedRoutine() {
			isMoving = enemy.rigidbody2D.linearVelocityX >= 0.01f;
			if (!isMoving) return;
			int velocitySign = (int)Mathf.Sign(enemy.rigidbody2D.linearVelocityX);
			int facingSign   = enemy.spriteRenderer.flipX ? 1 : - 1;
			movingBackward = velocitySign * facingSign == -1;
		}
	}
}