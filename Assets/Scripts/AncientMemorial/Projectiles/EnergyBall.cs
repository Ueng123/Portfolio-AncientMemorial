using System.Linq;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Projectiles {
	public class EnergyBall : Projectile {

		// 정적 프로퍼티
		private static readonly int BIG_IMPACT = "BigImpact".GetHash();

		// 인스턴스 프로퍼티
		private StopWatch  stopWatch;
		public  float      boomTime;

		// 인스턴스 메서드
		private void Boom() {
			Get(BIG_IMPACT, transform.position, PlayEffect: false);
			Release(PlayEffect: false);
		}

		// 오버라이드 메서드
		protected override void FixedRoutine() {
			rigidbody2D.linearVelocity = Vector2.right*(10*(boomTime-stopWatch.Tock())*(spriteRenderer.flipX?1:-1)/boomTime);
		}
		
		protected override void EarlyRoutine() {
			if (stopWatch.CheckIn(boomTime)) return;
			Boom();
		}

		protected override void Routine() { }

		protected override void LateRoutine() {
			CameraManager.instance.ShakeLerp(2, 5);
		}

		public override void Initialize() {
			base.Initialize();
			
			stopWatch = new StopWatch();
			stopWatch.Tick();
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner && !owner.isAttackTarget(entity)) return;

			SendAttackMultiplyEvent(entity, 1, useProcess: false, OnHit: Boom);
		}

		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Boom();
			}
		}
	}
}
