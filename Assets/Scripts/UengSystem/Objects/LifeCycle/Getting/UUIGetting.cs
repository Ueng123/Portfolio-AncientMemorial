using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class UUIGetting : Getting {

		// 인스턴스 프로퍼티
		private float AnimatorSpeed;
		private UUI   ui => (UUI)target;

		// 인스턴스 메서드
		public UUIGetting(UUI Target) : base(Target) { }

		// 오버라이드 메서드
		protected override void OnStartEffect() {
			if (!target.animator) return;
			AnimatorSpeed              = target.animator.speed;
			target.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			target.animator.speed      = 1;
			target.animator.Play(ui.gettingAnimation, 0, 0);
		}
		protected override void ClearEffect() {
			if (hasStartedEffect && target.animator) target.animator.speed = AnimatorSpeed;
			if (isComplete       && target.animator) target.animator.Play(ui.gettingAnimation, 0, 1);
		}
	}
}