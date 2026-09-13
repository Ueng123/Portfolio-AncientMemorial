using AncientMemorial.Entities;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Skeleton {
	public class SkeletonAttackReady : SkeletonState {

		// 인스턴스 프로퍼티
		private float targetDistance;
		private float margin;

		private StopWatch offsetTimer = new StopWatch();
		private float     updateOffsetTerm;
		
		private float offsetMax;
		private float offsetMin;
		private float offset;

		// 인스턴스 메서드
		public SkeletonAttackReady(float targetDistance, float margin, float offsetMax, float offsetMin, float updateOffsetTerm) {
			this.targetDistance   = targetDistance;
			this.margin           = margin;
			this.offsetMax        = offsetMax;
			this.offsetMin        = offsetMin;
			this.updateOffsetTerm = updateOffsetTerm;
		}
		
		private void UpdateOffset() {
			offset = Random.Range(offsetMin, offsetMax);
		}

		// 오버라이드 메서드
		public override void OnEnter() {
			offsetTimer.Tick();
			UpdateOffset();
		}

		public override void OnRoutine() {
			Entity target = stateMachine.getTarget.Invoke();
			
			if (offsetTimer.CheckOut(updateOffsetTerm)) {
				offsetTimer.Tick();
				UpdateOffset();
			}
			
			float positionX = enemy.transform.position.x;
			
			// + : 타겟이 -> 이쪽(+방향) : -방향으로 오프셋 붙어야 됨
			// ENEMY --------- OFFSETTED POSITION ----------- TARGET
			//   ---> signE                            <--- offset
			int   signE           = (int)Mathf.Sign(target.transform.position.x - enemy.transform.position.x);
			float targetPositionX = target.transform.position.x + offset - targetDistance*signE;

			if (Mathf.Abs(positionX - targetPositionX) <= margin) {
				stateMachine.Attack();
			}
			
			Move(targetPositionX, 0.2f, margin);
		}
	}
}
