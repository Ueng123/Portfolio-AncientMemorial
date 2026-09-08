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
			if (breakObject) UObject.Get(breakObject.name, transform.position, PlayEffect: false);
			Release(PlayEffect: false);
		}

		public override void Initialize() {
			base.Initialize();
			trailRenderer = GetComponent<TrailRenderer>();
			trailRenderer.Clear();
		}

		protected override void EarlyRoutine() { }

		protected override void Routine() { }

		protected override void LateRoutine() { base.LateRoutine(); }
		
		protected override void FixedRoutine() {
			if (isHitPending) return;
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rigidbody2D.linearVelocity.y, rigidbody2D.linearVelocity.x)*Mathf.Rad2Deg + rotateOffset);
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner) {
				if (!owner.isAttackTarget(entity)) return;
			}
			else {
				bool isTargetPlayer = targetPlayer && entity == Entity.player;
				bool isTargetEntity = targetEnemy  && entity != Entity.player;
				if (!isTargetPlayer && !isTargetEntity) return;
				
			}

			// 같은 물리 구간에서 예약된 타격도 관통 잔여 횟수에 포함함.
			bool CanPierce = !breakOnHit || hitableNum <= 0 || hitableNum > pendingHitCount + 1;
			SendCollisionHit(entity, damage, () => {
				if (breakOnHit && --hitableNum == 0) { Break(); return; }
				if (hitObject) Get(hitObject.name, transform.position, PlayEffect: false);
			}, CanPierce);
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break();
			}
		}
	}
}
