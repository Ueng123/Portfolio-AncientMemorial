using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using UengSystem.Audio;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UengSystem.UAction;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class CrystalRay : UObject {
		private static readonly int            End = Animator.StringToHash("end");
		private                 bool           rayShooting;
		public                  Entity         owner;
		public                  Transform      rayTransform;
		public                  Animator       rayAnimator;

		public bool       showEffect;
		public GameObject blackBG;
		private AudioSource ambientSource;

		private readonly List<RaycastHit2D>        results          = new();
		
		private readonly Dictionary<Entity, float> hitCooldownTable = new();
		private const    float                     damageThreshold  = 0.1f;
		
		private ContactFilter2D contactFilter;
		
		public        float radius;
		public        float angularVelocity;
		public        float angleOffset;
		private       float theta;
		private const float pi = 3.14159265358979323846f;
		
		protected override void FixedRoutine() {
			theta                      = Mathf.Repeat(theta + angularVelocity * (rayShooting?1:0.1f) * Time.fixedDeltaTime, 2*pi);
			float thetaUse             = theta + angleOffset;
			rayTransform.localPosition = new Vector3(radius*Mathf.Cos(thetaUse), radius*Mathf.Sin(thetaUse), 0);
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
					
					Entity.SendAttackMultiplyEvent(owner, entity, 1, false);
				}
			}
		}

		private const float rayShootAnimLength = 4.5f;
		private const float rayShootKeyframe   = 197f/270f;
		public const float rayShootTime = rayShootAnimLength * rayShootKeyframe;
		public override void Initialize() {
			base.Initialize();
			rayShooting = false;
			hitCooldownTable.Clear();
			rayAnimator.ResetTrigger(End);

			contactFilter = new ContactFilter2D { layerMask = LayerMask.GetMask("Entity"), useLayerMask = true };
			theta         = 0;
			
			blackBG.SetActive(showEffect);

			if (showEffect) PlaySFX("ultRaySpawn", volume:1f);
			new DelayedAction(rayShootTime, () => {
				rayShooting = true;

				if (showEffect) ambientSource = PlaySFX("ultRayAmbient", volume:1f, loop:true);
			}, () => { }, this).ExecuteDA();
		}
		
		public override float releasingDuration => 2;

		public override void OnFirstGet() {
			SetDefaultStates(ReleasingState: new RayReleasing(this));
			base.OnFirstGet();
		}

		protected override void OnRelease() {
			rayShooting = false;
			if (ambientSource && AudioManager.instance) {
				AudioManager.instance.StopSFX(ambientSource);
			}
			ambientSource = null;
			base.OnRelease();
		}

		private sealed class RayReleasing : Releasing {
			private CrystalRay ray => (CrystalRay)target;
			public RayReleasing(CrystalRay Target) : base(Target) { }
			protected override void OnStartEffect() {
				if (ray.showEffect && CameraBrain.instance) CameraBrain.instance.ZoomLerp(0.15f, 1);
				ray.rayAnimator.SetTrigger(End);
			}
			protected override void OnEffectRoutine(float DeltaTime) {
				ray.theta = Mathf.Repeat(ray.theta + ray.angularVelocity * DeltaTime, 2 * pi);
				float Theta = ray.theta + ray.angleOffset;
				ray.rayTransform.localPosition = new Vector3(ray.radius * Mathf.Cos(Theta), ray.radius * Mathf.Sin(Theta), 0);
				ray.rayTransform.localRotation = Quaternion.Euler(0, 0, 90 + Theta * Mathf.Rad2Deg);
			}
			protected override void ClearEffect() {
				ray.rayShooting = false;
				if (ray.blackBG) ray.blackBG.SetActive(false);
				if (ray.rayAnimator) ray.rayAnimator.ResetTrigger(End);
			}
		}
	}
}
