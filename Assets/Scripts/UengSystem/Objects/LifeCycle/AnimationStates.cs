using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class AnimationGetting : Getting {
		private readonly string AnimationName;
		private readonly string ActiveAnimation;
		private readonly float AnimationLength;
		private float InitialSpeed;
		public AnimationGetting(UObject Target, string AnimationName, string ActiveAnimation = null, float AnimationLength = 0) : base(Target) {
			this.AnimationName = AnimationName;
			this.ActiveAnimation = ActiveAnimation;
			this.AnimationLength = AnimationLength;
		}
		protected override void OnStartEffect() {
			if (!target.animator) return;
			InitialSpeed = target.animator.speed;
			target.animator.speed = AnimationLength > 0 && duration > 0 ? AnimationLength / duration : 1;
			target.animator.Play(AnimationName, 0, 0);
		}
		protected override void ClearEffect() {
			if (hasStartedEffect && target.animator) target.animator.speed = InitialSpeed;
			if (isComplete && target.animator && ActiveAnimation != null) target.animator.Play(ActiveAnimation, 0, 0);
		}
	}

	public class AnimationReleasing : Releasing {
		private readonly string AnimationName;
		private readonly float AnimationLength;
		private float InitialSpeed;
		public AnimationReleasing(UObject Target, string AnimationName, float AnimationLength = 0) : base(Target) {
			this.AnimationName = AnimationName;
			this.AnimationLength = AnimationLength;
		}
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
