using UengSystem.Utility;
using UengSystem.Objects;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using UengSystem;
using UengSystem.ObjectPool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities.Enemies {
	public abstract class Skeleton : Enemy {

		// 정적 프로퍼티
		private static readonly int DOOGAEGOL = "doogaegol".GetHash();
		private static readonly int SKELETON_EXCITED = "skeletonExcited".GetHash();
		private static readonly int SKELETON_DEATH = "skeletonDeath".GetHash();
		private static readonly int SKELETON_LAND = "skeletonLand".GetHash();

		protected static readonly int Landing   = Animator.StringToHash("landing");

		private static readonly int[] FootstepClipIds = { "footstep1".GetHash(), "footstep2".GetHash(), "footstep3".GetHash() };

		// 인스턴스 프로퍼티
		private bool _FootstepSound = false;
		private bool FootstepSound {
			get => _FootstepSound;
			set {
				if (_FootstepSound == value) return;
				
				if (value) PlaySFX(footstepClipId);
				_FootstepSound = value;
			}
		}
		protected virtual int footstepClipId => FootstepClipIds[Random.Range(0, FootstepClipIds.Length)];
		
		[SerializeField]
		private List<Sprite> footstepSprites;
		protected bool   isFootstep        => footstepSprites.Contains(spriteRenderer.sprite);

		// 오버라이드 메서드
		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			ShowDamageUI(damage);
			
			Stun(1);
			
			Vector2 pushDirection = pushDir??(attacker.transform.position - transform.position).normalized;
			float   velocity      = 0.5f + Mathf.Log(damage, 500);
			
			AddProcessToFixedUpdate(() => { rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse); });
			
			if (stat.HP > 0) {
				PlaySFX(SKELETON_EXCITED);
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraManager.instance.ShakeLerp(1f, 10f);
				CameraManager.instance.ZoomLerp(-0.1f);
			}
		}
		
		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}

		public override void Initialize() {
			base.Initialize();
		}

		protected override void Death() {
			PlaySFX(SKELETON_DEATH);

			GameManager.SetTimeScale(0f, 0.05f);
			CameraManager.instance.ShakeLerp(2f, 10);
			CameraManager.instance.ZoomLerp(-0.2f);

			GameObject doogaegol = UObject.Get(DOOGAEGOL, (Vector2)transform.position +  new Vector2(-0.03125f, 0.21875f), PlayEffect: false);
			Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
			
			doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
			doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);
			
			base.Death();
		}

		protected override void OnGrounded() {
			PlaySFX(SKELETON_LAND);
			
			Stun(0.5f);
		}
		
		protected override void LateRoutine() {
			FootstepSound = isFootstep;
			
			base.LateRoutine();
		}
	}
}
