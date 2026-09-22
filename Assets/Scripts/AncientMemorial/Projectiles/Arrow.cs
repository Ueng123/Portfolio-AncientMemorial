using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using AncientMemorial.Objects;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UActions;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Projectiles {
	public class Arrow : Projectile {

		// 정적 프로퍼티
		private static readonly int ARROW_DEBRIS = "ArrowDebris".GetHash();
		private static readonly int ARROW_HIT_EFFECT = "ArrowHitEffect".GetHash();

		// 인스턴스 프로퍼티
		private TrailRenderer trailRenderer;

		// 인스턴스 메서드
		private void Break(Transform parent = null, bool isEntity = false) {
			GameObject obj = UObject.Get(ARROW_DEBRIS, transform.position, PlayEffect: false);
			
			if (isEntity) {
				GameObject eff = UObject.Get(ARROW_HIT_EFFECT, transform.position, PlayEffect: false);
				eff.transform.rotation = transform.rotation;
			}
			
			if (parent) {
				if (isEntity) parent.GetComponent<Entity>().debrisAttached.Add(obj.GetComponent<AttatchObject>());
				obj.transform.SetParent(parent, true);
			}
			
			new DelayedAction(10, () => obj.GetComponent<UObject>().Release(PlayEffect: false), () => { }, obj.GetComponent<UObject>())
				.ExecuteDA();
			
			obj.transform.rotation = transform.rotation;
			Release(PlayEffect: false);
		}

		// 오버라이드 메서드
		public override void OnGet() {
			base.OnGet();
			rigidbody2D.centerOfMass = new Vector2(0.3f, 0);
			trailRenderer = GetComponent<TrailRenderer>();
		}

		public override void Initialize() {
			base.Initialize();
			trailRenderer = GetComponent<TrailRenderer>();
			trailRenderer.Clear();
		}

		protected override void FixedRoutine() {
			if (isHitPending) return;
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rigidbody2D.linearVelocity.y, rigidbody2D.linearVelocity.x)*Mathf.Rad2Deg);
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner && !owner.isAttackTarget(entity)) return;
			
			SendCollisionHit(entity, damage, () => Break(entity.transform, true));
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break(obj.transform);
			}
		}
	}
}
