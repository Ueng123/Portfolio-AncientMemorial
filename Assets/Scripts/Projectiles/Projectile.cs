using System.Collections.Generic;
using AncientMemorial.AMObjects;
using AncientMemorial.Entities;
using UnityEngine;
using Event = AncientMemorial.Events.Event;

namespace AncientMemorial.Projectiles {
	public abstract class Projectile : AdvancedAMObject {

		public Entity     owner;
		
		// will invoke on pi-hitted entity
		protected abstract void OnCollideEntity(Entity entity);
		
		protected abstract void OnCollideObject(AMObject obj);

		protected override void FixedRoutine() { }

		protected void OnTriggerEnter2D(Collider2D other) {
			Debug.Log(other.gameObject.name);
			
			Entity   entity = other.GetComponent<Entity>();
			if (entity) {
				OnCollideEntity(entity);
				return;
			}
			
			AMObject obj    = other.GetComponent<AMObject>();
			if (obj) OnCollideObject(obj);
		}

		public override    void OnEvent(Event e) { }
	}
}