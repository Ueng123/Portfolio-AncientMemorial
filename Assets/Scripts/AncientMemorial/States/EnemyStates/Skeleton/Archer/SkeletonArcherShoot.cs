using UengSystem.Utility;
using UengSystem.Objects;
using AncientMemorial.Entities;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Skeleton.Archer {
	public class SkeletonArcherShoot : SkeletonAttack {

		// 정적 프로퍼티
		private static readonly int ArrowPrefabId = "Arrow".GetHash();
		private static readonly int BowShootClipId = "bowShoot".GetHash();

		private const float attackProgress = 6f / 11f;

		// 인스턴스 프로퍼티
		public override float attackTime => 1.5f;
		
		private bool    isFlipped;
		private float   oldAnimSpeed;
		private Vector2 hitboxPos;
		private Vector2 hitboxSize;

		private int attackAnimation;

		private AttackArea _attackArea;

		// 인스턴스 메서드
		private float D(float v, Vector2 targetPosition) {
			float g  = Mathf.Abs(Physics2D.gravity.y);
			float x1 = targetPosition.x;
			float y1 = targetPosition.y;
			float x0 = enemy.transform.position.x;
			float y0 = enemy.transform.position.y;
			float xl = x1 - x0;
			float yl = y1 - y0;
			
			return xl * xl * v * v * v * v - xl * xl * xl * xl * g * g - 2 * xl * xl * yl * g * v * v;
		}

		private bool ArrowShootable(float v, Vector2 targetPosition) {
			return D(v, targetPosition) >= 0;
		}
		
		private Vector2 GetArrowDir(float v, Vector2 targetPosition) {
			float g  = Mathf.Abs(Physics2D.gravity.y);
			float x1 = targetPosition.x;
			float x0 = enemy.transform.position.x;
			float xl = x1 - x0;

			float   dirX = xl * v * v - Mathf.Sign(xl) * Mathf.Sqrt(D(v, targetPosition));
			float   dirY = xl * xl * g;
			Vector2 dir  = new(dirX, dirY);
			
			return dir.normalized * v;
		}

		private void ShootArrow(float v, Vector2 targetPosition) {
			if (!ArrowShootable(v, targetPosition)) return;
			
			enemy.PlaySFX(BowShootClipId);
			
			GameObject arrowObject = UObject.Get(ArrowPrefabId, enemy.transform.position, PlayEffect: false);
			Arrow      arrow       = arrowObject.GetComponent<Arrow>();
			
			arrow.rigidbody2D.linearVelocity = GetArrowDir(v, targetPosition);
			arrow.damage                     = enemy.data.attackDamage;
			arrow.owner                      = enemy;
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			Entity target = stateMachine.getTarget.Invoke();
			
			isFlipped    = (int)Mathf.Sign(target.transform.position.x - enemy.transform.position.x) == -1;
			
			oldAnimSpeed         = enemy.animator.speed;
			enemy.animator.speed = enemy.stat.attackSpeed;
			enemy.animator.SetBool(attacking, true);
		}
		
		public override void OnRoutine() {
			enemy.rigidbody2D.linearVelocityX = 0;
			Entity target = stateMachine.getTarget.Invoke();

			if (step == 0 && isProgress(0.5f)) {
				
				float velocity = Random.Range(10f, 11f);
				ShootArrow(velocity, target.transform.position);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(1)) {
				enemy.entityState = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();
			
			_attackArea?.Cancel();
			
			enemy.animator.speed = oldAnimSpeed;
			enemy.animator.SetBool(attacking, false);
		}
	}
}
