using AncientMemorial.Entities;
using AncientMemorial.Objects;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Skeleton.Warrior {
	public class SkeletonWarriorPierce : SkeletonAttack {
		public override float attackTime => 1.833f;
		
		private bool isFlipped;
		private float oldAnimSpeed;
		private Vector2 hitboxPos;
		private Vector2 hitboxSize;

		private int attackAnimation;

		private const float attackProgress = 6f / 11f;

		private AttackArea _attackArea;
		
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
				enemy.PlaySFX("swordSlash2");
				step = 1;
			}

			if (step == 1 && isProgress(1)) {
				_attackArea = null;
				enemy.state = GetState();
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