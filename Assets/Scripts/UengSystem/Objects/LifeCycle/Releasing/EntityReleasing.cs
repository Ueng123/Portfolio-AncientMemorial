using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class EntityReleasing : Releasing {

		// 인스턴스 프로퍼티
		private Color InitialColor;

		// 인스턴스 메서드
		public EntityReleasing(Entity Target) : base(Target) { }

		// 오버라이드 메서드
		protected override void OnStartEffect() {
			if (target.spriteRenderer) {
				InitialColor = target.spriteRenderer.color;
				Color Color = InitialColor;
				Color.a                     = 0;
				target.spriteRenderer.color = Color;
			}
			target.FreezeAnimator();
		}
		
		protected override void ClearEffect() {
			if (!hasStartedEffect) return;
			if (target.spriteRenderer) target.spriteRenderer.color = InitialColor;
			target.UnfreezeAnimator();
		}
	}
}