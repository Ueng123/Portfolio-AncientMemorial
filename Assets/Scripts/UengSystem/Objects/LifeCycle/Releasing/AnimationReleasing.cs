using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class AnimationReleasing : Releasing {

		// 인스턴스 프로퍼티
		private readonly string AnimationName;
		private readonly float AnimationLength;
		private float InitialSpeed;

		// 인스턴스 메서드
		public AnimationReleasing(UObject Target, string AnimationName, float AnimationLength = 0) : base(Target) {
			this.AnimationName = AnimationName;
			this.AnimationLength = AnimationLength;
		}

		// 오버라이드 메서드
		protected override void OnStartEffect() {
			if (!target.animator) return;
			InitialSpeed = target.animator.speed;
			target.animator.speed = AnimationLength > 0 && duration > 0 ? AnimationLength / duration : 1;
			target.animator.Play(AnimationName, 0, 0);
		}
		protected override void ClearEffect() {
			if (hasStartedEffect && target.animator) target.animator.speed = InitialSpeed;
		}
	}
}
