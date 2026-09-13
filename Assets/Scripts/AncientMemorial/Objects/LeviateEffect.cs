using System.Collections;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class LeviateEffect : UObject {

		// 인스턴스 프로퍼티
		public float leviateStep;

		// 오버라이드 메서드
		protected override void Routine() {
			transform.position += Vector3.up * (leviateStep * Time.deltaTime);
		}
		
		public override void OnFirstGet() {
			SetDefaultStates(ReleasingState: new LeviateReleasing(this));
			base.OnFirstGet();
		}

		// 중첩 타입
		private sealed class LeviateReleasing : DefaultReleasing {

			// 인스턴스 메서드
			public LeviateReleasing(LeviateEffect Target) : base(Target) { }

			// 오버라이드 메서드
			protected override void OnEffectRoutine(float DeltaTime) {
				target.transform.position += Vector3.up * (((LeviateEffect)target).leviateStep * DeltaTime);
				base.OnEffectRoutine(DeltaTime);
			}
		}
	}
}
