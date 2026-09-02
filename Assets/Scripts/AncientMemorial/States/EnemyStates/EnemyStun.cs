using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.States.EnemyStates {
	public class EnemyStun : EnemyState {

		private GameObject stunEffect;
		private float stunDuration;

		public EnemyStun Setup(float duration) {
			stunDuration = duration;
			return this;
		}
		
		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}
		
		public override void OnEnter() {
			enemy.rigidbody2D.linearVelocityX = 0;
			
			stunEffect = UObjectPool.instance.Get("StunEffect", enemy.transform.position + Vector3.up * enemy.markYPos);
			stunEffect.transform.SetParent(enemy.transform);
		}

		public override void OnEarlyRoutine() { }

		public override void OnRoutine() {
			if (stateTimer.CheckIn(stunDuration)) return;
			enemy.state = GetState();
		}

		public override void OnExit() {
			UObjectPool.instance.Release(stunEffect);
		}
	}
}
