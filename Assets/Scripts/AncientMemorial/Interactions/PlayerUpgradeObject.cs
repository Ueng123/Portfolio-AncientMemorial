using System.Collections;
using System.Collections.Generic;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;

namespace AncientMemorial.Interactions {
	public class PlayerUpgradeObject : Interaction {
		private bool                  selected = false;
		
		protected override void OnInteract() {
			selected             = true;
			List<UObject> playerUpgradeObjects = GetUObjects(Category);

			for (int i = playerUpgradeObjects.Count - 1; i >= 0; i--) {
				UObject upgradeObject = playerUpgradeObjects[i];
				UObjectPool.instance.Release(upgradeObject.gameObject, 1f);
			}
		}

		protected override void PrepareSpawnFX() { }
		
		protected override IEnumerator SpawnFX(float duration) {
			animator.Play("spawn");
			animator.speed = 2f/duration;
			
			yield return new WaitForSeconds(duration);
		}

		protected override void        FinishSpawnFX() {
			animator.Play("idle");
			animator.speed = 1;
			
			Initialize();
		}

		protected override void        PrepareDespawnFX() { }
		protected override IEnumerator DespawnFX(float duration) {
			animator.Play(selected?"select":"break");
			animator.speed = 1f/duration;
			selected       = false;
			
			yield return new WaitForSeconds(duration);
			
			UObjectPool.instance.Release(gameObject, -1);
		}
	}
}