using AncientMemorial.AMObjects;
using AncientMemorial.Entities;
using AncientMemorial.Events;
using AncientMemorial.Events.EventDatas;
using AncientMemorial.ObjectPool;
using UnityEngine;

using EventType = AncientMemorial.Events.EventType;

namespace AncientMemorial.Projectiles {
	public class Arrow : Projectile {
		
		public override void OnGet() {
			rigidbody2D.centerOfMass = new Vector2(0.3f, 0);
		}

		private void Fierce() {
			// GET() DEBRIS
			AMObjectPool.instance.Release(gameObject);
		}

		private void Hit() {
			// GET() HIT EFFECT
			AMObjectPool.instance.Release(gameObject);
		}
		
		protected override void EarlyRoutine() { }

		protected override void Routine() { }

		protected override void LateRoutine() { }

		protected override void OnCollideEntity(Entity entity) {
			if (owner.team == entity.team) return;
			
			SendEvent(EventType.Entity_Behaviour_Hit, new EntityHitData(owner, entity, this));
		}
		
		protected override void OnCollideObject(AMObject obj) {
			if (obj.GetComponent<Standable>()) {
				Fierce();
			}
		}
	}
}