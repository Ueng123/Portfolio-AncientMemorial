using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class DefaultReleasing : Releasing {
		private Color InitialColor;
		private Sprite InitialSprite;
		public DefaultReleasing(UObject Target) : base(Target) { }

		protected override void OnStartEffect() {
			if (target.spriteRenderer) {
				InitialColor = target.spriteRenderer.color;
				InitialSprite = target.spriteRenderer.sprite;
				target.spriteRenderer.sprite = target.whiteSpawnSprite ? target.whiteSpawnSprite : InitialSprite;
			}
			target.FreezeAnimator();
		}

		protected override void OnEffectRoutine(float DeltaTime) {
			if (!target.spriteRenderer) return;
			Color Color = GameManager.instance ? GameManager.instance.spawnColor : Color.white;
			Color.a = duration <= 0 ? 0 : 1 - Mathf.Clamp01(elapsed / duration);
			target.spriteRenderer.color = Color;
		}

		protected override void ClearEffect() {
			if (!hasStartedEffect) return;
			if (target.spriteRenderer) {
				target.spriteRenderer.color = InitialColor;
				target.spriteRenderer.sprite = InitialSprite;
			}
			target.UnfreezeAnimator();
		}
	}
}
