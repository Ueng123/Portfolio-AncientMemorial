using AncientMemorial.Entities;
using System;
using UengSystem.Events;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.UDebug;
using UengSystem.Utility;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Projectiles {
	public abstract class Projectile : UObject {

		// 인스턴스 프로퍼티
		public Entity owner;
		public float  damage;

		// 정적 메서드
		// OnHit은 HitData.onHit에 연결되어 실제 타격이 적용될 때 실행됨.
		public static void SendAttackMultiplyEvent(Entity owner, Projectile attacker, Entity target, float damageMult, bool ignoreInvincible = false, bool useProcess = true, Action OnHit = null) {
			UObject eventSender = owner.To<UObject>() ?? GlobalObject.instance.To<UObject>();
			
			HitData hitData = new (
				null,
				attacker,
				target,
				attacker.damage * damageMult,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
			hitData.onHit = OnHit;
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");

			if (useProcess) {
				GlobalObject.instance.AddProcessToUpdate(() => { eventSender?.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData); });
			}
			else {
				eventSender?.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData);
			}
		}
		
		public static void SendAttackEvent(Entity owner, Projectile attacker, Entity target, float damage, bool ignoreInvincible = false, bool useProcess = true, Action OnHit = null) {
			UObject eventSender = owner.To<UObject>() ?? GlobalObject.instance.To<UObject>();
			
			HitData hitData = new (
				null,
				attacker,
				target,
				damage,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
			hitData.onHit = OnHit;
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");

			if (useProcess) {
				GlobalObject.instance.AddProcessToUpdate(() => { eventSender?.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData); });
			}
			else {
				eventSender?.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData);
			}
		}
		
		protected abstract void OnCollideEntity(Entity entity);
		protected abstract void OnCollideObject(UObject obj);

		protected void OnTriggerEnter2D(Collider2D other) {
			if (isReleased) return;
			
			Entity   entity = other.GetComponent<Entity>();
			if (entity) {
				OnCollideEntity(entity);
				return;
			}
			
			UObject obj    = other.GetComponent<UObject>();
			if (obj) OnCollideObject(obj);
		}
		
		public void SendAttackMultiplyEvent(Entity target, float damageMult, bool ignoreInvincible = false, bool useProcess = true, Action OnHit = null) {
			SendAttackMultiplyEvent(owner, this, target, damageMult, ignoreInvincible, useProcess, OnHit);
		}
		
		public void SendAttackEvent(Entity target, float damage, bool ignoreInvincible = false, bool useProcess = true, Action OnHit = null) {
			SendAttackEvent(owner, this, target, damage, ignoreInvincible, useProcess, OnHit);
		}
	}
}
