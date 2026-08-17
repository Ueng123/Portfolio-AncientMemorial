using System.Collections;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.Objects {
	public class UObjectAnimator : UObject {
		public AnimationClip GetClip;
		public AnimationClip InitializeClip;
		public AnimationClip ReleaseClip;

		protected override void        PrepareSpawnFX() {
			FreezeRigidbody2D();
		}

		protected override IEnumerator SpawnFX(float duration) {
			animator.Play(GetClip.name);
			yield return new WaitForSeconds(GetClip.length);
		}

		public override void Initialize() {
			animator.Play(InitializeClip.name);
			base.Initialize();
		}

		protected override void        FinishSpawnFX() { 
			UnfreezeRigidbody2D();
			Initialize();
		}

		protected override void        PrepareDespawnFX() {
			FreezeRigidbody2D();
		}

		protected override IEnumerator DespawnFX(float duration) {
			animator.Play(ReleaseClip.name);
			yield return new WaitForSeconds(GetClip.length);
			
			UnfreezeRigidbody2D();
			
			UObjectPool.instance.Release(gameObject, -1);
		}
	}
}