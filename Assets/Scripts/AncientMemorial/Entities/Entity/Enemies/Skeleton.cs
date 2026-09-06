using UengSystem.Objects;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using UengSystem;
using UengSystem.ObjectPool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities.Enemies {
	public abstract class Skeleton : Enemy {
		protected static readonly int Landing   = Animator.StringToHash("landing");

		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			ShowDamageUI(damage);
			
			Stun(1);
			
			Vector2 pushDirection = pushDir??(attacker.transform.position - transform.position).normalized;
			float   velocity      = 0.5f + Mathf.Log(damage, 500);
			
			AddProcessToFixedUpdate(() => { rigidbody2D.AddForce(pushDirection * velocity, ForceMode2D.Impulse); });
			
			if (stat.HP > 0) {
				PlaySFX("skeletonExcited");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0f, 0.05f);
				CameraBrain.instance.ShakeLerp(1f, 10f);
				CameraBrain.instance.ZoomLerp(-0.1f);
			}
		}
		
		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}

		public override void Initialize() {
			base.Initialize();
		}

		protected override void Death() {
			PlaySFX("skeletonDeath");

			GameManager.SetTimeScale(0f, 0.05f);
			CameraBrain.instance.ShakeLerp(2f, 10);
			CameraBrain.instance.ZoomLerp(-0.2f);

			GameObject doogaegol = UObject.Get("doogaegol", (Vector2)transform.position +  new Vector2(-0.03125f, 0.21875f), PlayEffect: false);
			Rigidbody2D doogaegolRB = doogaegol.GetComponent<Rigidbody2D>();
			
			doogaegolRB.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(4f, 6f)), ForceMode2D.Impulse);
			doogaegolRB.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);
			
			base.Death();
		}

		protected override void OnGrounded() {
			PlaySFX("skeletonLand");
			
			Stun(0.5f);
		}

		private bool _FootstepSound = false;
		private bool FootstepSound {
			get => _FootstepSound;
			set {
				if (_FootstepSound == value) return;
				
				if (value) PlaySFX(footstepSoundName);
				_FootstepSound = value;
			}
		}

		private readonly string[] footstepSoundNames  = { "footstep1", "footstep2", "footstep3" };
		protected virtual string footstepSoundName => footstepSoundNames[Random.Range(0, footstepSoundNames.Length)];
		
		[SerializeField]
		private List<Sprite> footstepSprites;
		protected bool   isFootstep        => footstepSprites.Contains(spriteRenderer.sprite);
		
		protected override void LateRoutine() {
			FootstepSound = isFootstep;
			
			base.LateRoutine();
		}
	}
}
