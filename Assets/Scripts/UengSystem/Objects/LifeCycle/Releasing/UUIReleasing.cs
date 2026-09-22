using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class UUIReleasing : Releasing {

		// 정적 프로퍼티
		private static readonly int   Close = Animator.StringToHash("Close");

		// 인스턴스 프로퍼티
		private                 float AnimatorSpeed;

		// 인스턴스 메서드
		public UUIReleasing(UUI Target) : base(Target) { }

		// 오버라이드 메서드
		protected override void OnStartEffect() {
			if (!target.animator) return;
			AnimatorSpeed              = target.animator.speed;
			target.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			target.animator.speed      = 1;
			target.animator.SetTrigger(Close);
		}
		
		protected override void ClearEffect() {
			if (hasStartedEffect && target.animator) target.animator.speed = AnimatorSpeed;
			if (target.animator) target.animator.ResetTrigger(Close);
		}
	}
}