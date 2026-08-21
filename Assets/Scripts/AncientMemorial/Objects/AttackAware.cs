using System;
using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Objects {
	public class AttackAware : UObject {
		public SpriteRenderer whiteObject;
		public Vector2    targetSize;
		
		public Color targetColor;

		public bool lerpX = true;
		public bool lerpY = true;
		
		private StopWatch stopWatch;
		public float duration = 1;

		public Entity attacker;
		public float  damageMult;
		public int    maxTargetNum;
		public bool   ignoreInvincible;

		public bool invisible;
		
		public void Cancel() {
			UObjectPool.instance.Release(gameObject, invisible?0:0.1f);
		}
		
		public void Damage() {
			Cancel();
			
			if (damageMult == 0) return;
			
			Vector2 hitboxPos  = transform.position;
			Vector2 hitboxSize = targetSize;
			float   angle      = transform.localEulerAngles.z;
			
			Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitboxPos, hitboxSize, angle);
			foreach (Collider2D hit in hitColliders) {
				if (!hit.CompareTag("Entity")) continue;
				Entity entity     = hit.GetComponent<Entity>();
						
				if (!attacker.isAttackTarget(entity)) continue;

				attacker.SendAttackMultiplyEvent(entity, damageMult, ignoreInvincible);
						
				if (--maxTargetNum == 0) return;
			}
		}
		
		protected override void Routine() {
			if (!(stopWatch?.Check(duration) ?? true)) {
				Damage();
			}

			if (invisible) return;
			
			whiteObject.size     = new Vector2(targetSize.x * Mathf.Clamp01(lerpX&&duration !=0 ? stopWatch.Tock() / duration : 1),
											   targetSize.y * Mathf.Clamp01(lerpY&&duration !=0 ? stopWatch.Tock() / duration : 1));
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * (1 /duration) * 5f);
			whiteObject.color    = spriteRenderer.color;
		}
		
		public override void Initialize() {
			base.Initialize();
			stopWatch = new StopWatch();
			stopWatch.Tick();
			
			whiteObject.size     = Vector2.zero;
			spriteRenderer.size  = targetSize;
		}

		public void InitializeColor(Color color) {
			spriteRenderer.color = color;
			whiteObject.color    = color;
		}
	}
}