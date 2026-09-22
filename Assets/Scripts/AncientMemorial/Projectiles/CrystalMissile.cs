using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Projectiles {
	public class CrystalMissile : Projectile {

		// 정적 프로퍼티
		private static readonly int LOCK_ON_EFFECT = "LockOnEffect".GetHash();

		// 인스턴스 프로퍼티
		public float TimeBeforeLockOn;
		public float LockOnDuration;
		public float initialSpeed;

		private bool _lockOn;
		private bool lockOn {
			get => _lockOn;
			set {
				if (value == _lockOn) return;

				if (value) {
					GameObject eff = UObject.Get(LOCK_ON_EFFECT, transform.position, PlayEffect: false);
					eff.transform.rotation = transform.rotation;
				}

				_lockOn = value;
			}
		}
		
		private TrailRenderer trailRenderer;

		public float offset;
		private Vector2 targetPosition;
		
		private StopWatch stopWatch = new ();
		
		private bool locking;
		private bool locked;

		// 인스턴스 메서드
		private void Break() {
			Release(PlayEffect: false);
		}
		private void LockOn() {
			if (!Entity.player) return;
			
			Vector2 plrPos = Entity.player.transform.position;
			
			targetPosition = Vector2.up*0.25f + plrPos;
			
			float currAngle = transform.eulerAngles.z;
			float targetAngle = Mathf.Atan2(
									targetPosition.y - transform.position.y,
									targetPosition.x - transform.position.x
								) * Mathf.Rad2Deg - 90f;
			
			transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(currAngle, targetAngle + offset, Time.deltaTime*5/LockOnDuration));
		}

		private float GetVelocity() {
			if (lockOn) return initialSpeed / 2;
			float a = initialSpeed + 1f;
			float b = stopWatch.Tock() - (LockOnDuration / 2 + TimeBeforeLockOn);
			return a - initialSpeed / (b * b * 0.1f + 1);
		}

		// 오버라이드 메서드
		public override void OnGet() {
			base.OnGet();
			rigidbody2D.centerOfMass = new Vector2(0.3f, 0);
			trailRenderer = GetComponent<TrailRenderer>();
			stopWatch.Tick();

			locking = true;
			offset  = Random.Range(-5f, 5f);
		}

		public override void Initialize() {
			base.Initialize();
			trailRenderer = GetComponent<TrailRenderer>();
			trailRenderer.Clear();
		}

		protected override void EarlyRoutine() { }
		protected override void Routine() {
			if (isHitPending) return;
			lockOn = stopWatch.CheckOut(LockOnDuration + TimeBeforeLockOn);
			if (stopWatch.CheckIn(LockOnDuration + TimeBeforeLockOn) && stopWatch.CheckOut(TimeBeforeLockOn)) { LockOn(); }
			
			rigidbody2D.linearVelocity = transform.up * GetVelocity();
		}

		protected override void LateRoutine() { base.LateRoutine(); }
		protected override void FixedRoutine() {
			if (isHitPending) return;
			if (locking) return;
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rigidbody2D.linearVelocity.y, rigidbody2D.linearVelocity.x)*Mathf.Rad2Deg);
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner && !owner.isAttackTarget(entity)) return;
			
			SendCollisionHit(entity, damage, Break);
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break();
			}
		}
	}
}
