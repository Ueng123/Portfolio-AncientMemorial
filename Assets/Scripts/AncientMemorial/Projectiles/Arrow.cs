using AncientMemorial.Entities;
using UengSystem.Events.EventDatas;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Projectiles {
	public class Arrow : Projectile {
		
		public override void OnGet() {
			rigidbody2D.centerOfMass = new Vector2(0.3f, 0);
		}

		private void Fierce() {
			// GET() DEBRIS
			UObjectPool.instance.Release(gameObject);
		}

		private void Hit() {
			// GET() HIT EFFECT
			UObjectPool.instance.Release(gameObject);
		}
		
		protected override void EarlyRoutine() { }

		protected override void Routine() { }

		protected override void LateRoutine() { }

		protected override void OnCollideEntity(Entity entity) {
			if (owner.team == entity.team) return;
			
			SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, new EntityHitData(owner, entity, this));
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<Standable>()) {
				Fierce();
			}
		}
	}
}