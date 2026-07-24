using AncientMemorial.Entities;
using AncientMemorial.Objects;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Projectiles {
	public class CrystalMissile : Projectile {

		public float TimeBeforeLockOn;
		public float LockOnDuration;
		public float initialSpeed;
		public bool  expectAttack;
		public bool  noLockOn;

		private bool _lockOn;
		private bool lockOn {
			get => _lockOn;
			set {
				if (value == _lockOn) return;

				if (value) {
					GameObject eff = UObjectPool.instance.Get("LockOnEffect", transform.position);
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
			UObjectPool.instance.Release(gameObject);
		}

		public float offset;
		private Vector2 targetPosition;
		private void LockOn() {
			if (!Entity.player) return;
			
			Vector2 plrPos = Entity.player.transform.position;
			
			targetPosition = Vector2.up*0.25f
							 + (expectAttack ?
									Entity.player.GetExpectPos(((Vector2)transform.position - plrPos).magnitude * 2f / initialSpeed) :
									plrPos);
			
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
		
		private StopWatch stopWatch = new ();
		protected override void Routine() {
			if (noLockOn) return;
			lockOn = !stopWatch.Check(LockOnDuration + TimeBeforeLockOn);
			if (stopWatch.Check(LockOnDuration + TimeBeforeLockOn) && !stopWatch.Check(TimeBeforeLockOn)) { LockOn(); }
			rigidbody2D.linearVelocity = transform.up *
										 (!lockOn
											  ? initialSpeed+1f - initialSpeed /
												(Mathf.Pow(stopWatch.Tock() - (LockOnDuration / 2 + TimeBeforeLockOn),
														   2) * 0.1f + 1)
											  : initialSpeed/2);
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
			
			SendEvent(EventType.Entity_Behaviour_Hit, 10, new EntityHitData(
						  null,
						  this,
						  entity,
						  damage,
						  new Vector2(entity.transform.position.x - transform.position.x, 0).normalized
					  ));
			
			Break();
		}
		
		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Break();
			}
		}
	}
}