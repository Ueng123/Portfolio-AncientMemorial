using System.Collections;
using AncientMemorial.Map;
using UengSystem.Objects;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class WoodStructure : UObject {
		private void MatchWithMapSize() {
			transform.position  = new Vector2(transform.position.x, .5f+MapManager.instance.GetMapSize().y/2);
			spriteRenderer.size = new Vector2(spriteRenderer.size.x,    MapManager.instance.GetMapSize().y);
		}

		public override void OnGet() {
			MatchWithMapSize();
		}

		protected override IEnumerator SpawnFX(float duration) {
			float elapsed = 0f;

			while (elapsed < duration) {
				elapsed += Time.deltaTime;
				
				float t = elapsed / duration;

				Color color = new(Mathf.Lerp(GameManager.instance.spawnColor.r, 1, t),
								  Mathf.Lerp(GameManager.instance.spawnColor.g, 1, t),
								  Mathf.Lerp(GameManager.instance.spawnColor.b, 1, t),
								  Mathf.Lerp(0,                                 1, t));
				
				MatchWithMapSize();
				if (spriteRenderer) spriteRenderer.color = color;
				
				yield return null;
			}
		}

		protected override void EarlyRoutine() {
			MatchWithMapSize();
		}
	}
}