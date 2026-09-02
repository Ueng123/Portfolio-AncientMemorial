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
		private StopWatch  stopWatch;
		public  float      boomTime;

		protected override void FixedRoutine() {
			rigidbody2D.linearVelocity = Vector2.right*(10*(boomTime-stopWatch.Tock())*(spriteRenderer.flipX?1:-1)/boomTime);
		}

		private void Boom() {
			UObjectPool.instance.Get("BigImpact", transform.position);
			UObjectPool.instance.Release(gameObject);
		}
		
		protected override void EarlyRoutine() {
			if (stopWatch.CheckIn(boomTime)) return;
			Boom();
		}

		protected override void Routine() { }

		protected override void LateRoutine() {
			CameraBrain.instance.ShakeLerp(2, 0);
		}

		public override void Initialize() {
			base.Initialize();
			
			stopWatch = new StopWatch();
			stopWatch.Tick();
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner && !owner.isAttackTarget(entity)) return;

			SendAttackMultiplyEvent(entity, 1);
			Boom();
		}

		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Boom();
			}
		}
	}
}
