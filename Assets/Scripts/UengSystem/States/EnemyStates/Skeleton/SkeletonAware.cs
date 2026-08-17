using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Skeleton {
	public class SkeletonAware : SkeletonState {

		public SkeletonAware(float targetDistance, float margin, float offsetMax, float offsetMin, float updateOffsetTerm) {
			this.targetDistance = targetDistance;
			this.margin = margin;
			this.offsetMax = offsetMax;
			this.offsetMin = offsetMin;
			this.updateOffsetTerm = updateOffsetTerm;
		}
		
		private float targetDistance;
		private float margin;

		private StopWatch offsetTimer = new StopWatch();
		private float     updateOffsetTerm;
		
		private float     offsetMax;
		private float     offsetMin;
		private float     offset;
		
		private void UpdateOffset() {
			offset = Random.Range(offsetMin, offsetMax);
		}

		public override void OnEnter() {
			offsetTimer.Tick();
			UpdateOffset();
		}

		public override void OnRoutine() {
			if (!offsetTimer.Check(updateOffsetTerm)) {
				offsetTimer.Tick();
				UpdateOffset();
			}
			
			// + : 타겟이 -> 이쪽(+방향) : -방향으로 오프셋 붙어야 됨
			// ENEMY --------- OFFSETTED POSITION ----------- TARGET
			//   ---> signE                            <--- offset
			int   signE           = (int)Mathf.Sign(target.transform.position.x - enemy.transform.position.x);
			float targetPositionX = target.transform.position.x + offset - targetDistance*signE;
			
			Move(targetPositionX, 0.2f, margin);
		}
	}
}