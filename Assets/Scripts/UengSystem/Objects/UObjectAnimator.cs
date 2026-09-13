using System.Collections;
using UengSystem.ObjectPool;
using UengSystem.Objects.LifeCycle;
using UnityEngine;

namespace UengSystem.Objects {
	public class UObjectAnimator : UObject {

		// 인스턴스 프로퍼티
		public AnimationClip GetClip;
		public AnimationClip InitializeClip;
		public AnimationClip ReleaseClip;

		public override float gettingDuration => GetClip ? GetClip.length : 0;
		public override float releasingDuration => ReleaseClip ? ReleaseClip.length : 0;

		// 오버라이드 메서드
		public override void OnFirstGet() {
			AnimationGetting getting = new (this, GetClip ? GetClip.name : "");
			AnimationReleasing releasing = new (this, ReleaseClip ? ReleaseClip.name : "");

			SetDefaultStates(getting, releasing);
			base.OnFirstGet();
		}

		public override void Initialize() {
			if (animator && InitializeClip) animator.Play(InitializeClip.name, 0, 0);
			base.Initialize();
		}
	}
}
