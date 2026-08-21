using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.States.EnemyStates {
	public class EnemyStun : EnemyState {

		private GameObject stunEffect;
		private float stunDuration;

		public void SetDuration(float targetDuration) {
			stunDuration = targetDuration;
		}
		
		public override void OnEnter() {
			base.OnEnter();

			enemy.rigidbody2D.linearVelocityX = 0;
			
			stunEffect = UObjectPool.instance.Get("StunEffect", enemy.transform.position + Vector3.up * enemy.markYPos);
			stunEffect.transform.SetParent(enemy.transform);
		}

		public override void OnEarlyRoutine() { }

		public override void OnRoutine() {
			if (stateTimer.Check(stunDuration)) return;
			enemy.state = GetState();
		}

		public override void OnExit() {
			base.OnExit();
			UObjectPool.instance.Release(stunEffect);
		}
	}
}