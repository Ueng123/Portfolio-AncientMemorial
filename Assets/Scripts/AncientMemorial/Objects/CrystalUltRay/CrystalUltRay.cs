using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Objects {
	public class CrystalUltRay : AdvancedUObject {
		private static readonly int            End = Animator.StringToHash("end");
		private                 bool           rayShooting;
		public                  Entity         owner;
		public                  Transform      rayTransform;
		public                  Animator       rayAnimator;

		public bool       showEffect;
		public GameObject blackBG;

		private readonly List<RaycastHit2D>        results          = new();
		
		private readonly Dictionary<Entity, float> hitCooldownTable = new();
		private const    float                     damageThreshold  = 0.1f;
		
		private ContactFilter2D contactFilter;
		
		public        float r;
		public        float w;
		public        float o;
		private       float theta;
		private const float pi = 3.14159265358979323846f;
		
		protected override void EarlyRoutine() { }
		protected override void Routine() { }
		protected override void LateRoutine() { }

		protected override void FixedRoutine() {
			theta                      = Mathf.Repeat(theta + w * (rayShooting?1:0.3f) * Time.fixedDeltaTime, 2*pi);
			float thetaUse             = theta + o;
			rayTransform.localPosition = new Vector3(r*Mathf.Cos(thetaUse), r*Mathf.Sin(thetaUse), 0);
			rayTransform.localRotation = Quaternion.Euler(0f, 0f, 90 + thetaUse*Mathf.Rad2Deg);
			
			if (!rayShooting) return;

			if (showEffect) {
				CameraBrain.instance.ShakeLerp(5, 1);
				CameraBrain.instance.ZoomLerp(0.15f);
			}
			
			float dist = MapManager.instance.GetMapSize().magnitude + 1f; // 1f는 그래도 혹시모를 이유로 안전상 붙힘
			Vector2 dir = -rayTransform.up;
			Vector2 rightDir = rayTransform.right;
			
			const float d      = 1.375f;
			const int   rayNum = 12;
			Vector2     dn     = rightDir * d;

			Vector2 startPos = (Vector2)rayTransform.position - dn/2f;
			
			for (int i = 0; i < rayNum; i++) {
				Vector2 rayStartPos = startPos + dn * (i / (rayNum - 1f));
				Debug.DrawLine(rayStartPos, rayStartPos+dir*dist, Color.black);
				
				int hitCount = Physics2D.Raycast(rayStartPos, dir, contactFilter, results, dist);
				if (hitCount == 0) { continue; }
				
				foreach (RaycastHit2D hit in results) {
					Debug.DrawLine(rayStartPos, hit.point, Color.red);
					
					Entity entity = hit.transform.GetComponent<Entity>();

					if (!owner) continue;
					if (!owner.isAttackTarget(entity)) continue;
					
					if (hitCooldownTable.TryGetValue(entity, out float lastHitTime)) {
						if (Time.fixedTime - lastHitTime < damageThreshold) continue;
					}
					
					hitCooldownTable[entity] = Time.fixedTime;
					
					Entity.SendAttackEvent(owner, entity, 3, false);
				}
			}
		}

		private const float rayShootAnimLength = 4.5f;
		private const float rayShootKeyframe   = 197f/270f;

		public const float rayShootTime = rayShootAnimLength * rayShootKeyframe;
		public override void Initialize() {
			base.Initialize();

			contactFilter = new ContactFilter2D { layerMask = LayerMask.GetMask("Entity"), useLayerMask = true };
			theta         = 0;
			
			blackBG.SetActive(showEffect);
			
			new DelayedAction(rayShootTime, () => {
				rayShooting = true;
			}, () => { }, this).Execute();
		}
		
		private const float despawnAnimTime = 2f;
		private StopWatch stopWatch = new ();

		protected override IEnumerator DespawnFX(float duration) {
			
			ToggleColliders(false);

			if (rigidbody2D) {
				rigidbody2D.bodyType        = RigidbodyType2D.Kinematic;
				rigidbody2D.linearVelocity  = Vector2.zero;
				rigidbody2D.angularVelocity = 0;
			}
			
			CameraBrain.instance.ZoomLerp(0.15f, 1);
			rayShooting = false;
			rayAnimator.SetTrigger(End);
			stopWatch.Tick();
			
			while (stopWatch.Check(despawnAnimTime)) {
				theta                      = Mathf.Repeat(theta + w * DeltaTime, 2*pi);
				float thetaUse             = theta + o;
				rayTransform.localPosition = new Vector3(r*Mathf.Cos(thetaUse), r*Mathf.Sin(thetaUse), 0);
				rayTransform.localRotation = Quaternion.Euler(0f, 0f, 90 + thetaUse*Mathf.Rad2Deg);
				yield return null;
			}
			
			UObjectPool.instance.Release(gameObject, -1);
		}
	}
}