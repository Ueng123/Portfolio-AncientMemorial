using System;
using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Projectiles {
	public class BasicProjectile : Projectile {

		// 인스턴스 프로퍼티
		private int BreakPrefabId;
		private int HitPrefabId;

		public float rotateOffset;
		
		public bool targetPlayer;
		public bool targetEnemy;

		public GameObject hitObject;
		public GameObject breakObject;

		public bool breakOnHit;
		public int  hitableNum;
		
		private TrailRenderer trailRenderer;

		// 인스턴스 메서드
		private void Break() {
			if (breakObject) Get(BreakPrefabId, transform.position, PlayEffect: false);
			Release(PlayEffect: false);
		}

		private void Hit() {
			if (hitObject) Get(HitPrefabId, transform.position, PlayEffect: false);
			if (breakOnHit && --hitableNum == 0) Break();
		}

		// 오버라이드 메서드
		protected override void OnGet() {
			base.OnGet();
			trailRenderer = GetComponent<TrailRenderer>();
		}

		public override void Initialize() {
			base.Initialize();
			BreakPrefabId = breakObject ? breakObject.name.GetHash() : 0;
			HitPrefabId = hitObject ? hitObject.name.GetHash() : 0;
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
			}
			else {
				bool isTargetPlayer = targetPlayer && entity == Entity.player;
				bool isTargetEntity = targetEnemy  && entity != Entity.player;
				if (!isTargetPlayer && !isTargetEntity) return;
			}
			
			if (owner) SendAttackMultiplyEvent(entity, damage, useProcess: false, OnHit: Hit);
			else SendAttackEvent(entity, damage, useProcess: false, OnHit: Hit);
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break();
			}
		}
	}
}
