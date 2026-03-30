using AncientMemorial.AMObjects;
using AncientMemorial.Entities;
using AncientMemorial.Events;
using AncientMemorial.Events.EventDatas;
using UnityEngine;

using EventType = AncientMemorial.Events.EventType;

namespace AncientMemorial.Projectiles {
	public class Arrow : Projectile {

		protected override void EarlyRoutine() { }

		protected override void Routine() { }

		protected override void LateRoutine() { }

		protected override void OnCollideEntity(Entity entity) {
			if (owner.team == entity.team) return;
			
			SendEvent(EventType.Entity_Behaviour_Hit, new EntityHitData(owner, entity, this));
		}

		protected override void OnCollideObject(AMObject obj) {
			
		}

		protected override void OnCollideMap() {
			
		}
	}
}