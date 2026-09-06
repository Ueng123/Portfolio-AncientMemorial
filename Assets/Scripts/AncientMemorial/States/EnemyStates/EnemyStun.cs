using UengSystem.Objects;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates {
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
			
			stunEffect = UObject.Get("StunEffect", enemy.transform.position + Vector3.up * enemy.markYPos, PlayEffect: false);
			stunEffect.transform.SetParent(enemy.transform);
		}

		public override void OnEarlyRoutine() { }

		public override void OnRoutine() {
			if (stateTimer.CheckIn(stunDuration)) return;
			enemy.entityState = GetState();
		}

		public override void OnExit() {
			stunEffect.GetComponent<UObject>().Release(PlayEffect: false);
		}
	}
}
