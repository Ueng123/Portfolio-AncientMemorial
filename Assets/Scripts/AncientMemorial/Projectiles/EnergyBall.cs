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
			UObject.Get(BIG_IMPACT, transform.position, PlayEffect: false);
			Release(PlayEffect: false);
		}

		// 오버라이드 메서드
		protected override void FixedRoutine() {
			if (isHitPending) return;
			rigidbody2D.linearVelocity = Vector2.right*(10*(boomTime-stopWatch.Tock())*(spriteRenderer.flipX?1:-1)/boomTime);
		}
		
		protected override void EarlyRoutine() {
			if (isHitPending) return;
			if (stopWatch.CheckIn(boomTime)) return;
			Boom();
		}

		protected override void Routine() { }

		protected override void LateRoutine() {
			base.LateRoutine();
			if (!isActive) return;
			CameraManager.instance.ShakeLerp(2, 0);
		}

		public override void Initialize() {
			base.Initialize();
			
			stopWatch = new StopWatch();
			stopWatch.Tick();
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner && !owner.isAttackTarget(entity)) return;

			SendCollisionHit(entity, damage, Boom);
		}

		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Boom();
			}
		}
	}
}
