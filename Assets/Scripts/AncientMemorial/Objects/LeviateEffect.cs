using System.Collections;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class LeviateEffect : AdvancedUObject {
		public float leviateStep;
		
		protected override void EarlyRoutine() { }

		protected override void Routine() {
			transform.position += Vector3.up * (leviateStep * DeltaTime);
		}

		protected override void LateRoutine() { }

		protected override void FixedRoutine() { }

		protected override IEnumerator DespawnFX(float duration) {
			float elapsed    = 0f;
			
			spriteRenderer.sprite = whiteSpawnSprite??spriteRenderer.sprite;
			
			while (elapsed < duration) {
				elapsed            += DeltaTime;
				transform.position += Vector3.up * (leviateStep * DeltaTime);
				float t = elapsed                               / duration;

				Color color = new (GameManager.instance.spawnColor.r,
								   GameManager.instance.spawnColor.g,
								   GameManager.instance.spawnColor.b,
								   Mathf.Lerp(1, 0, t));

				spriteRenderer.color = color;

				yield return null;
			}
			
			spriteRenderer.color = Color.white;
			UObjectPool.instance.Release(gameObject, -1);
		}
	}
}