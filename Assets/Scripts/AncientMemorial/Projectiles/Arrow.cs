using AncientMemorial.Entities;
using AncientMemorial.Objects;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Projectiles {
	public class Arrow : Projectile {

		private TrailRenderer trailRenderer;
		
		public override void OnGet() {
			base.OnGet();
			rigidbody2D.centerOfMass = new Vector2(0.3f, 0);
			trailRenderer = GetComponent<TrailRenderer>();
		}

		private void Break(Transform parent = null, bool isEntity = false) {
			GameObject obj = UObjectPool.instance.Get("ArrowDebris", transform.position);
			
			if (isEntity) {
				GameObject eff = UObjectPool.instance.Get("ArrowHitEffect", transform.position);
				eff.transform.rotation = transform.rotation;
			}
			
			if (parent) {
				if (isEntity) parent.GetComponent<Entity>().debrisAttached.Add(obj.GetComponent<Debris>());
				obj.transform.SetParent(parent, true);
			}
			
			new DelayedAction(10, () => UObjectPool.instance.Release(obj), () => { }, obj.GetComponent<UObject>())
				.ExecuteDA();
			
			obj.transform.rotation = transform.rotation;
			UObjectPool.instance.Release(gameObject);
		}

		public override void Initialize() {
			base.Initialize();
			trailRenderer = GetComponent<TrailRenderer>();
			trailRenderer.Clear();
		}

		protected override void EarlyRoutine() { }

		protected override void Routine() { }

		protected override void LateRoutine() { }

		protected override void FixedRoutine() {
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rigidbody2D.linearVelocity.y, rigidbody2D.linearVelocity.x)*Mathf.Rad2Deg);
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner) {
				if (owner.entityType == EntityType.Player && entity.Friendly) return;
				if (owner.entityType != EntityType.Player && !((Enemy)owner).isTargettable(entity.entityType)) return;
			}
			
			SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, 10, new EntityHitData(
						  null,
						  this,
						  entity,
						  damage*owner.entityStat.attackDamage,
						  new Vector2(entity.transform.position.x - transform.position.x, 0).normalized
					  ));
			
			Break(entity.transform, true);
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break(obj.transform);
			}
		}
	}
}