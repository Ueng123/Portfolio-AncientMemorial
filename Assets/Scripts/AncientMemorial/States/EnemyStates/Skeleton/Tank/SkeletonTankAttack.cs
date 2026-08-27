using System;
using AncientMemorial;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace UengSystem.States.EnemyStates.Skeleton.Tank {
	public abstract class SkeletonTankAttack : SkeletonAttack {
		private const float weakDuration = .3f;
		
		protected StopWatch weakStopwatch = new StopWatch();
		protected bool weak => weakStopwatch.Check(weakDuration);
		
		protected void Weak() {
			UObjectPool.instance.Get("groggyEffect", enemy.transform.position);
			weakStopwatch.Tick();
			
			GameManager.SetTimeScale(0.34f, 0.9f);
			CameraBrain.instance.ZoomLerp(-0.5f);
		}

		protected readonly Action<Event> tryGroggy;
		protected void TryGroggy(Event e) {
			if (e.GetData<HitData>().reciever != enemy) return;
			if (!weak) return;
			
			Groggy();
		}
		
		protected void Groggy() {
			enemy.PlaySFX("tankGroggy");
			
			// Process를 통해 다음 프레임 시작시 이벤트를 추가하므로 for문중 배열 변경 문제 없음.
			Entity.SendAttackEvent(enemy, enemy, enemy.entityData.HP/15, true);
			
			GameManager.SetTimeScale(0.05f, 0.75f);
			CameraBrain.instance.ShakeLerp(5f, 3);
			CameraBrain.instance.ZoomLerp(-1.25f);
			
			enemy.Stun(5);
		}

		protected SkeletonTankAttack() {
			tryGroggy = TryGroggy;
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