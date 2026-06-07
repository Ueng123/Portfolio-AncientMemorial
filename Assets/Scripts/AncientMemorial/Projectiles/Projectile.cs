using AncientMemorial.Entities;
using UengSystem.Objects;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;

namespace AncientMemorial.Projectiles {
	public abstract class Projectile : AdvancedUObject {

		public Entity owner;
		public float  damage;
		
		// will invoke on pi-hitted entity
		protected abstract void OnCollideEntity(Entity entity);
		
		protected abstract void OnCollideObject(UObject obj);

		protected override void FixedRoutine() { }

		protected void OnTriggerEnter2D(Collider2D other) {
			//Debug.Log(other.gameObject.name);
			if (isReleased) return;
			
			Entity   entity = other.GetComponent<Entity>();
			if (entity) {
				OnCollideEntity(entity);
				return;
			}
			
			UObject obj    = other.GetComponent<UObject>();
			if (obj) OnCollideObject(obj);
		}

		public override    void OnEvent(Events_Event e) { }
	}
}