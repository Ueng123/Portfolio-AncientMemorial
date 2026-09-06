using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Projectiles {
	public class CrystalMissile : Projectile {

		public float TimeBeforeLockOn;
		public float LockOnDuration;
		public float initialSpeed;

		private bool _lockOn;
		private bool lockOn {
			get => _lockOn;
			set {
				if (value == _lockOn) return;

				if (value) {
					GameObject eff = UObject.Get("LockOnEffect", transform.position, PlayEffect: false);
					eff.transform.rotation = transform.rotation;
				}

				_lockOn = value;
			}
		}
		
		private TrailRenderer trailRenderer;
		public override void OnGet() {
			base.OnGet();
			rigidbody2D.centerOfMass = new Vector2(0.3f, 0);
			trailRenderer = GetComponent<TrailRenderer>();
			stopWatch.Tick();

			locking = true;
			offset  = Random.Range(-5f, 5f);
		}

		private void Break() {
			Release(PlayEffect: false);
		}

		public float offset;
		private Vector2 targetPosition;
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

		public override void Initialize() {
			base.Initialize();
			trailRenderer = GetComponent<TrailRenderer>();
			trailRenderer.Clear();
		}

		protected override void EarlyRoutine() { }

		private float GetVelocity() {
			if (lockOn) return initialSpeed / 2;
			float a = initialSpeed + 1f;
			float b = stopWatch.Tock() - (LockOnDuration / 2 + TimeBeforeLockOn);
			return a - initialSpeed / (b * b * 0.1f + 1);
		}
		
		private StopWatch stopWatch = new ();
		protected override void Routine() {
			lockOn = stopWatch.CheckOut(LockOnDuration + TimeBeforeLockOn);
			if (stopWatch.CheckIn(LockOnDuration + TimeBeforeLockOn) && stopWatch.CheckOut(TimeBeforeLockOn)) { LockOn(); }
			
			rigidbody2D.linearVelocity = transform.up * GetVelocity();
		}

		protected override void LateRoutine() { }
		
		private bool locking;
		private bool locked;
		protected override void FixedRoutine() {
			if (locking) return;
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rigidbody2D.linearVelocity.y, rigidbody2D.linearVelocity.x)*Mathf.Rad2Deg);
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner) { if (!owner.isAttackTarget(entity)) return; }
			
			SendAttackEvent(entity, damage);
			
			Break();
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break();
			}
		}
	}
}
