using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.Objects;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;

namespace AncientMemorial.Projectiles {
	public abstract class Projectile : UObject {

		public Entity owner;
		public float  damage;
		
		// will invoke on pi-hitted entity
		protected abstract void OnCollideEntity(Entity entity);
		
		protected abstract void OnCollideObject(UObject obj);

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

		protected void SendAttackMultiplyEvent(Entity targetEntity, float damageMult) {
			SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, (int)EventPriority.Hit, new EntityHitData(
						  null,
						  this,
						  targetEntity,
						  damage * damageMult,
						  new Vector2(targetEntity.transform.position.x - transform.position.x, 0).normalized
					  ));
		}

		protected void SendAttackEvent(Entity targetEntity, float damageMult) {
			SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, (int)EventPriority.Hit, new EntityHitData(
						  null,
						  this,
						  targetEntity,
						  damageMult,
						  new Vector2(targetEntity.transform.position.x - transform.position.x, 0).normalized
					  ));
		}
	}
}