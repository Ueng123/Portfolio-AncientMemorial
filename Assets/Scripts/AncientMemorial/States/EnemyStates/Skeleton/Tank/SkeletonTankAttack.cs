using UengSystem.Objects;
using System;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using UengSystem;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.States.EnemyStates.Skeleton.Tank {
	public abstract class SkeletonTankAttack : SkeletonAttack {

		// 정적 프로퍼티
		private static readonly int GroggyEffectPrefabId = "groggyEffect".GetHash();
		private static readonly int TankGroggyClipId = "tankGroggy".GetHash();

		private const float weakDuration = .3f;

		// 인스턴스 프로퍼티
		protected StopWatch weakStopwatch = new StopWatch();
		protected bool weak => weakStopwatch.CheckIn(weakDuration);

		protected readonly Action<Event> tryGroggy;

		// 인스턴스 메서드
		protected void Weak() {
			UObject.Get(GroggyEffectPrefabId, enemy.transform.position, PlayEffect: false);
			weakStopwatch.Tick();
			
			GameManager.SetTimeScale(0.34f, 0.9f);
			CameraBrain.instance.ZoomLerp(-0.5f);
		}
		protected void TryGroggy(Event e) {
			if (e.GetData<HitData>().reciever != enemy) return;
			if (!weak) return;
			
			Groggy();
		}
		
		protected void Groggy() {
			enemy.PlaySFX(TankGroggyClipId);
			
			// Process를 통해 다음 프레임 시작시 이벤트를 추가하므로 for문중 배열 변경 문제 없음.
			Entity.SendAttackEvent(enemy, enemy, enemy.data.HP/15, true);
			
			GameManager.SetTimeScale(0.05f, 0.75f);
			CameraBrain.instance.ShakeLerp(5f, 3);
			CameraBrain.instance.ZoomLerp(-1.25f);
			
			enemy.Stun(5);
		}

		protected SkeletonTankAttack() {
			tryGroggy = TryGroggy;
		}

		// 오버라이드 메서드
		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}

		public override void OnEnter() {
			base.OnEnter();
			EventType.Entity_Hit.AddListener(tryGroggy);
		}

		public override void OnExit() {
			base.OnExit();
			EventType.Entity_Hit.RemoveListener(tryGroggy);
		}
	}
}
