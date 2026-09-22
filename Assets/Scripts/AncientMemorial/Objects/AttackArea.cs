using System;
using AncientMemorial.Entities;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Objects {
	public class AttackArea : UObject {

		// 인스턴스 프로퍼티
		public SpriteRenderer whiteObject;
		public Vector2    targetSize;
		
		public Color targetColor;

		public bool lerpX = true;
		public bool lerpY = true;
		
		private StopWatch stopWatch;
		public float duration = 1;

		public Entity attacker;
		private long AttackerLife;
		private bool hasActiveAttacker => attacker && attacker.isActive && attacker.Matches(AttackerLife);
		public float  damageMult;
		public int    maxTargetNum;
		public bool   ignoreInvincible;

		public bool invisible;

		// 인스턴스 메서드
		public void Cancel() {
			Release(PlayEffect: !invisible);
		}
		
		public void Damage() {
			if (!isActive || !hasActiveAttacker) { Cancel(); return; }
			if (damageMult == 0) { Cancel(); return; }
			
			Vector2 hitboxPos  = transform.position;
			Vector2 hitboxSize = targetSize;
			float   angle      = transform.localEulerAngles.z;
			
			Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitboxPos, hitboxSize, angle);
			foreach (Collider2D hit in hitColliders) {
				if (!hit.CompareTag("Entity")) continue;
				Entity entity     = hit.GetComponent<Entity>();
						
				if (!attacker.isAttackTarget(entity)) continue;

				attacker.SendAttackMultiplyEvent(entity, damageMult, ignoreInvincible);
						
				if (--maxTargetNum == 0) break;
			}
			Cancel(); // Last operation: this may synchronously return the instance to its pool.
		}

		public void InitializeColor(Color color) {
			spriteRenderer.color = color;
			whiteObject.color    = color;
		}

		// 오버라이드 메서드
		protected override void Routine() {
			if (!hasActiveAttacker) { Cancel(); return; }
			if (stopWatch?.CheckOut(duration) ?? false) {
				Damage();
				return;
			}

			if (invisible) return;
			
			whiteObject.size     = new Vector2(targetSize.x * Mathf.Clamp01(lerpX&&duration !=0 ? stopWatch.Tock() / duration : 1),
											   targetSize.y * Mathf.Clamp01(lerpY&&duration !=0 ? stopWatch.Tock() / duration : 1));
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * (1 /duration) * 5f);
			whiteObject.color    = spriteRenderer.color;
		}
		
		public override void Initialize() {
			base.Initialize();
			AttackerLife = attacker ? attacker.lifeNumber : 0;
			if (attacker) attacker.RegisterAttackArea(this);
			stopWatch = new StopWatch();
			stopWatch.Tick();

			if (invisible) {
				spriteRenderer.color = new Color(0, 0, 0, 0);
				whiteObject.color    = new Color(0, 0, 0, 0);
			}
			
			whiteObject.size     = Vector2.zero;
			spriteRenderer.size  = targetSize;
		}

		protected override void OnRelease() {
			if (attacker && attacker.Matches(AttackerLife)) attacker.UnregisterAttackArea(this);
			base.OnRelease();
		}
	}
}
