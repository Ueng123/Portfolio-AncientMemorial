using UengSystem.Utility;
using AncientMemorial.Entities;
using AncientMemorial.Objects;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Skeleton.Warrior {
	public class SkeletonWarriorPierce : SkeletonAttack {

		// 정적 프로퍼티
		private static readonly int SwordSlash2ClipId = "swordSlash2".GetHash();

		private const float attackProgress = 6f / 11f;

		// 인스턴스 프로퍼티
		public override float attackTime => 1.833f;
		
		private bool isFlipped;
		private float oldAnimSpeed;
		private Vector2 hitboxPos;
		private Vector2 hitboxSize;

		private int attackAnimation;

		private AttackArea _attackArea;

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			Entity target = stateMachine.getTarget.Invoke();
			
			isFlipped    = (int)Mathf.Sign(target.transform.position.x - enemy.transform.position.x) == -1;
			
			oldAnimSpeed         = enemy.animator.speed;
			enemy.animator.speed = enemy.stat.attackSpeed;
			
			attackAnimation = isFlipped ? attackF : attackB;
			enemy.animator.SetBool(attackAnimation, true);
			enemy.animator.SetTrigger(attacking);
			
			hitboxPos   = (Vector2)enemy.transform.position + new Vector2(0.5395f * (isFlipped ? -1 : 1), -0.155f);
			hitboxSize  = new Vector2(1.46f, 0.31f);
			_attackArea = Entity.AttackArea(enemy, 1, GetDelay(attackProgress), hitboxPos, hitboxSize, 0, 1, false, true);
		}
		
		public override void OnRoutine() {
			enemy.rigidbody2D.linearVelocityX = 0;
			
			if (step == 0 && isProgress(attackProgress)) {
				enemy.PlaySFX(SwordSlash2ClipId);
				step = 1;
			}

			if (step == 1 && isProgress(1)) {
				_attackArea = null;
				enemy.entityState = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();
			
			_attackArea?.Cancel();
			
			enemy.animator.speed = oldAnimSpeed;
			enemy.animator.SetBool(attackAnimation, false);
			enemy.animator.SetTrigger(attacking);
		}
	}
}