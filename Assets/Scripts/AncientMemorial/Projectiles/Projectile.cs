using AncientMemorial.Entities;
using System;
using System.Collections.Generic;
using UengSystem.Events;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.UDebug;
using UengSystem.Utility;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Projectiles {
	public abstract class Projectile : UObject {

		// 인스턴스 프로퍼티
		public Entity owner;
		public float  damage;

		private readonly Queue<PendingHit> PendingHits = new();
		private bool StopAfterHit;
		private bool ResumePhysics;
		private Action PendingObjectCollision;
		protected bool isHitPending => PendingHits.Count > 0;
		protected int pendingHitCount => PendingHits.Count;

		// 정적 메서드
		public static void SendAttackMultiplyEvent(Entity owner, Projectile attacker, Entity target, float damageMult, bool ignoreInvincible = false, bool useProcess = true) {
			UObject eventSender = owner.To<UObject>() ?? GlobalObject.instance.To<UObject>();
			
			HitData hitData = new (
				null,
				attacker,
				target,
				attacker.damage * damageMult,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");
			
			if (useProcess) {
				GlobalObject.instance.AddProcessToUpdate(()=> { eventSender.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData); });
			}
			else {
				eventSender.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData);
			}
		}
		
		public static void SendAttackEvent(Entity owner, Projectile attacker, Entity target, float damage, bool ignoreInvincible = false, bool useProcess = true) {
			UObject eventSender = owner.To<UObject>() ?? GlobalObject.instance.To<UObject>();
			
			HitData hitData = new (
				null,
				attacker,
				target,
				damage,
				new Vector2(target.transform.position.x - attacker.transform.position.x, 0).normalized,
				ignoreInvincible
			);
					
			DebugManager.Log($"[HIT EVENT] SendingEvent : {target}");
			
			if (useProcess) {
				GlobalObject.instance.AddProcessToUpdate(()=> { eventSender.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData); });
			}
			else {
				eventSender.SendEvent(EventType.Entity_Hit, EventPriority.Hit, hitData);
			}
		}

		// 인스턴스 메서드
		// 충돌 당시 정보를 저장하고, 자신의 Update에서 전송할 때까지 투사체를 유지함.
		protected void SendCollisionHit(Entity Target, float Damage, Action OnHit, bool CanPierce = false) {
			if (!isActive || StopAfterHit) return;
			foreach (PendingHit Pending in PendingHits) {
				if (Pending.Target == Target && Target.Matches(Pending.TargetLife)) return;
			}

			long Life = lifeNumber;
			PendingHit Hit = new() {
				Target = Target,
				TargetLife = Target.lifeNumber,
				// 무적/무효/리스너 미등록 시에도 전송한 프레임의 LateRoutine에서 반환함.
				Effect = () => Release(PlayEffect: false)
			};
			
			HitData Data = new(null, this, Target, Damage,
				new Vector2(Target.transform.position.x - transform.position.x, 0).normalized) {
				onHit = () => {
					if (!Matches(Life) || !isActive) return;
					// 성공 콜백은 실행할 연출을 선택함. 다른 이벤트 리스너까지 읽은 후 반환함.
					Hit.Effect = () => {
						if (Target && Target.isActive && Target.Matches(Hit.TargetLife)) OnHit();
						else Release(PlayEffect: false);
					};
				}
			};
			UObject Sender = owner ? owner : GlobalObject.instance;
			if (!isHitPending) {
				ResumePhysics = true;
				FreezeRigidbody2D();
			}
			PendingHits.Enqueue(Hit);
			StopAfterHit = !CanPierce;
			AddProcessToUpdate(() => {
				if (!Matches(Life) || !isActive) return;
				Hit.Dispatched = true;
				if (Sender) Sender.SendEvent(EventType.Entity_Hit, EventPriority.Hit, Data);
			});
		}
		
		// will invoke on pi-hitted entity
		protected abstract void OnCollideEntity(Entity entity);
		
		protected abstract void OnCollideObject(UObject obj);

		protected void OnTriggerEnter2D(Collider2D other) {
			//Debug.Log(other.gameObject.name);
			if (!isActive || StopAfterHit) return;
			
			Entity   entity = other.GetComponent<Entity>();
			if (entity) {
				OnCollideEntity(entity);
				return;
			}
			
			UObject obj    = other.GetComponent<UObject>();
			if (!obj) return;
			if (isHitPending) {
				// 관통 타격과 같은 물리 구간에서 벽에 닿아도 예약된 피해부터 처리함.
				long ObjectLife = obj.lifeNumber;
				PendingObjectCollision = () => {
					if (obj && obj.Matches(ObjectLife) && !obj.isReleased) OnCollideObject(obj);
				};
				StopAfterHit = true;
				return;
			}
			OnCollideObject(obj);
		}
		
		public void SendAttackMultiplyEvent(Entity target, float damageMult, bool ignoreInvincible = false, bool useProcess = true) {
			SendAttackMultiplyEvent(owner, this, target, damageMult, ignoreInvincible, useProcess);
		}
		
		public void SendAttackEvent(Entity target, float damage, bool ignoreInvincible = false, bool useProcess = true) {
			SendAttackEvent(owner, this, target, damage, ignoreInvincible, useProcess);
		}

		// 오버라이드 메서드
		public override void OnGet() {
			PendingHits.Clear();
			PendingObjectCollision = null;
			StopAfterHit = false;
			ResumePhysics = false;
			base.OnGet();
		}

		protected override void OnRelease() {
			PendingHits.Clear();
			PendingObjectCollision = null;
			base.OnRelease();
		}

		protected override void LateRoutine() {
			base.LateRoutine();
			long Life = lifeNumber;
			try {
				while (isActive && Matches(Life) && isHitPending && PendingHits.Peek().Dispatched) {
					PendingHits.Dequeue().Effect.Invoke();
				}
				if (!isActive || !Matches(Life) || isHitPending) return;
				Action ObjectCollision = PendingObjectCollision;
				PendingObjectCollision = null;
				ObjectCollision?.Invoke();
			}
			catch {
				if (isActive && Matches(Life)) Release(PlayEffect: false);
				throw;
			}
			finally {
				if (isActive && Matches(Life) && !isHitPending) {
					StopAfterHit = false;
					if (ResumePhysics) UnfreezeRigidbody2D();
					ResumePhysics = false;
				}
			}
		}

		// 중첩 타입
		private sealed class PendingHit {

			// 인스턴스 프로퍼티
			public Entity Target;
			public long TargetLife;
			public bool Dispatched;
			public Action Effect;
		}
	}
}
