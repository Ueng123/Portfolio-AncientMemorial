using AncientMemorial.Entities;
using UengSystem.Events.EventDatas;
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

		private void Break() {
			GameObject obj  = UObjectPool.instance.Get("ArrowDebris", transform.position);
			obj.transform.rotation = transform.rotation;
			
			UObjectPool.instance.Release(gameObject);
		}

		private void Hit() {
			// GET() HIT EFFECT
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
			if (owner && owner.team == entity.team) return;
			
			SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, 10, new EntityHitData(owner, entity, this));
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break();
			}
		}
	}
}