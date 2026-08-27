using AncientMemorial.Map;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Skeleton {
	public class SkeletonWander : SkeletonState {
		
		public SkeletonWander(float maxOffsetSize, float updatePositionTerm) {
			this.maxOffsetSize = maxOffsetSize;
			this.updatePositionTerm = updatePositionTerm;
		}
		
		private float maxOffsetSize;
		private float wanderOffsetMin => Mathf.Clamp(enemy.transform.position.x - MapManager.leftWall,  0, maxOffsetSize); // 왼쪽 벽으로부터 얼마나 떨어져있는지
		private float wanderOffsetMax => Mathf.Clamp(MapManager.rightWall - enemy.transform.position.x, 0, maxOffsetSize); // 오른쪽 벽으로부터 얼마나 떨어져있는지
		
		private StopWatch wanderingTimer = new StopWatch();
		private float randomMapPosX;
		private float updatePositionTerm;

		
		private void ResetRandomMapPos() {
			float offset = Random.Range(-wanderOffsetMin, wanderOffsetMax);
			randomMapPosX = enemy.transform.position.x + offset;
		}
		
		public override void OnEnter() {
			wanderingTimer.Tick();
			ResetRandomMapPos();
		}

		public override void OnRoutine() {
			if (!wanderingTimer.Check(updatePositionTerm)) {
				wanderingTimer.Tick();
				ResetRandomMapPos();
			}
			
			Move(randomMapPosX, 0.2f, 0.1f);
		}
	}
}