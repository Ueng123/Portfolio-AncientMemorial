using System.Linq;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Objects;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Projectiles {
	public class EnergyBall : Projectile {
		private StopWatch  stopWatch;
		public  float      boomTime;
		public  Vector2    hitboxSize;
		public  GameObject awareObject;
		
		private void Boom() {
			UObjectPool.instance.Get("BigImpact", transform.position);
			
			float distCoEfficient = Entity.player?Mathf.Clamp01(Vector2.Distance(transform.position, Entity.player.transform.position)*5):1;
			CameraBrain.instance.ShakeLerp(10f*distCoEfficient, 10f);
			
			UObjectPool.instance.Release(gameObject);
			
			Collider2D[] hitColliders = Physics2D.OverlapBoxAll(transform.position, hitboxSize, 0f);
			foreach (Collider2D hit in hitColliders) {
				if (!hit.CompareTag("Entity")) continue;
				Entity entity = hit.GetComponent<Entity>();
				
				if (entity == owner) continue;
				if (!((Enemy)owner).aggroC.Keys.ToList().Contains(entity.entityType)) continue;
				
				Debug.Log($"[HIT EVENT] SendingEvent : {entity}");
				AddProcessToUpdate(()=>SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, 10, new EntityHitData(
													 owner,
													 null,
													 entity,
													 damage*2/3,
													 new Vector2(entity.transform.position.x - transform.position.x, 0).normalized
												 )));
			}
		}

		private void Penetrate(Entity entity) { }

		protected override void FixedRoutine() {
			rigidbody2D.linearVelocity = Vector2.right*(10*(boomTime-stopWatch.Tock())*(spriteRenderer.flipX?1:-1)/boomTime);
		}

		protected override void EarlyRoutine() { }

		protected override void Routine() {
			if (!stopWatch.Check(boomTime)) Boom();
		}

		protected override void LateRoutine() {
			CameraBrain.instance.ShakeLerp(2, 0);
		}

		public override void Initialize() {
			base.Initialize();
			
			stopWatch = new StopWatch();
			stopWatch.Tick();
			
			awareObject = UObjectPool.instance.Get("AttackAware", transform.position);
			awareObject.transform.SetParent(transform, true);
			AttackAware attackAware         = awareObject.GetComponent<AttackAware>();
			attackAware.spriteRenderer.size = new Vector2(0,            0);
			attackAware.targetSize          = new Vector2(hitboxSize.x, hitboxSize.y);
			attackAware.duration            = boomTime;
		}

		protected override void OnCollideEntity(Entity entity) {
			if (owner) {
				if (owner.entityType == EntityType.Player && entity.Friendly) return;
				if (owner.entityType != EntityType.Player && !((Enemy)owner).isTargettable(entity.entityType)) return;
			}
			
			SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, 10, new EntityHitData(
						  null,
						  this,
						  entity,
						  damage,
						  new Vector2(entity.transform.position.x - transform.position.x, 0).normalized
					  ));
			
			Penetrate(entity);
		}

		protected override void OnRelease() {
			base.OnRelease();
			UObjectPool.instance.Release(awareObject);
		}

		protected override void OnCollideObject(UObject obj) {
			if (obj.GetComponent<ProjectileBrokeable>()) {
				Boom();
			}
		}
	}
}