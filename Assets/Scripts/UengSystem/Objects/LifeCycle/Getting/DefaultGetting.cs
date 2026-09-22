using UengSystem.Utility;
using AncientMemorial.Objects;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class DefaultGetting : Getting {

		// 정적 프로퍼티
		private static readonly int SPAWN_EFFECT = "SpawnEffect".GetHash();

		// 인스턴스 프로퍼티
		private Color InitialColor;
		private UObject SpawnEffect;
		private long SpawnLife;

		// 인스턴스 메서드
		public DefaultGetting(UObject Target) : base(Target) { }

		// 오버라이드 메서드
		protected override void OnStartEffect() {
			SpawnEffect = null;
			if (target.spriteRenderer) {
				InitialColor = target.spriteRenderer.color;
			}
			target.FreezeAnimator();
			
			SpawnEffect = UObject.Get(SPAWN_EFFECT, target.transform.position, false,
					Effect => Effect.GetComponent<SpawnEffectHelper>().t_s = duration).GetComponent<UObject>();
			SpawnLife = SpawnEffect.lifeNumber;
			
			OnEffectRoutine(0);
		}

		protected override void OnEffectRoutine(float DeltaTime) {
			if (!target.spriteRenderer) return;
			float Progress = duration <= 0 ? 1 : Mathf.Clamp01(elapsed / duration);
			Color Color = InitialColor;
			Color.a *= Progress;
			target.spriteRenderer.color = Color;
		}

		protected override void ClearEffect() {
			if (!hasStartedEffect) return;
			if (SpawnEffect && SpawnEffect.Matches(SpawnLife)) SpawnEffect.Release(false);
			SpawnEffect = null;
			if (target.spriteRenderer) {
				target.spriteRenderer.color = InitialColor;
			}
			target.UnfreezeAnimator();
		}
	}
}
