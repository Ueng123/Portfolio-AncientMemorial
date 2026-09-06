using System.Collections;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class LeviateEffect : UObject {
		public float leviateStep;

		protected override void Routine() {
			transform.position += Vector3.up * (leviateStep * Time.deltaTime);
		}
		
		public override void OnFirstGet() {
			SetDefaultStates(ReleasingState: new LeviateReleasing(this));
			base.OnFirstGet();
		}

		private sealed class LeviateReleasing : DefaultReleasing {
			public LeviateReleasing(LeviateEffect Target) : base(Target) { }
			protected override void OnEffectRoutine(float DeltaTime) {
				target.transform.position += Vector3.up * (((LeviateEffect)target).leviateStep * DeltaTime);
				base.OnEffectRoutine(DeltaTime);
			}
		}
	}
}
