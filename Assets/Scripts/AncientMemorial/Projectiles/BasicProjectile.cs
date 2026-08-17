using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Projectiles {
	public class BasicProjectile : Projectile {

		public float rotateOffset;
		
		public bool targetPlayer;
		public bool targetEnemy;

		public GameObject hitObject;
		public GameObject breakObject;

		public bool breakOnHit;
		public int  hitableNum;
		
		private TrailRenderer trailRenderer;
		
		public override void OnGet() {
			base.OnGet();
			trailRenderer = GetComponent<TrailRenderer>();
		}

		private void Break() {
			if (breakObject) UObjectPool.instance.Get(breakObject.name, transform.position);
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
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rigidbody2D.linearVelocity.y, rigidbody2D.linearVelocity.x)*Mathf.Rad2Deg + rotateOffset);
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner) {
				if (!owner.isAttackTarget(entity)) return;
				SendAttackMultiplyEvent(entity, damage);
			}
			else {
				bool isTargetPlayer = targetPlayer && entity == Entity.player;
				bool isTargetEntity = targetEnemy  && entity != Entity.player;
				if (!isTargetPlayer && !isTargetEntity) return;
				
				SendAttackEvent(entity, damage);
			}

			if (breakOnHit && --hitableNum == 0) { Break(); return; }
			if (hitObject) UObjectPool.instance.Get(hitObject.name, transform.position);
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break();
			}
		}
	}
}