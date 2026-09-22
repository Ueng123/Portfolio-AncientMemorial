using UengSystem.Utility;
using UengSystem.Objects;
using UengSystem.ObjectPool;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates {
	public class EnemyStun : EnemyState {

		// 정적 프로퍼티
		private static readonly int STUN_EFFECT = "StunEffect".GetHash();

		// 인스턴스 프로퍼티
		private GameObject stunEffect;
		private float stunDuration;

		// 인스턴스 메서드
		public EnemyStun Setup(float duration) {
			stunDuration = duration;
			return this;
		}

		// 오버라이드 메서드
		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}
		
		public override void OnEnter() {
			enemy.rigidbody2D.linearVelocityX = 0;
			
			stunEffect = UObject.Get(STUN_EFFECT, enemy.transform.position + Vector3.up * enemy.markYPos, PlayEffect: false);
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
