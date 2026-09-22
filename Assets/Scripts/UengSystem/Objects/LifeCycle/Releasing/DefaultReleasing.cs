using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class DefaultReleasing : Releasing {

		// 인스턴스 프로퍼티
		private Color InitialColor;

		// 인스턴스 메서드
		public DefaultReleasing(UObject Target) : base(Target) { }

		// 오버라이드 메서드
		protected override void OnStartEffect() {
			if (target.spriteRenderer) {
				InitialColor = target.spriteRenderer.color;
			}
			target.FreezeAnimator();
		}

		protected override void OnEffectRoutine(float DeltaTime) {
			if (!target.spriteRenderer) return;
			Color Color = InitialColor;
			Color.a *= duration <= 0 ? 0 : 1 - Mathf.Clamp01(elapsed / duration);
			target.spriteRenderer.color = Color;
		}

		protected override void ClearEffect() {
			if (!hasStartedEffect) return;
			if (target.spriteRenderer) {
				target.spriteRenderer.color = InitialColor;
			}
			target.UnfreezeAnimator();
		}
	}
}
