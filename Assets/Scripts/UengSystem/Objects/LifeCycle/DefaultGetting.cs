using UengSystem.Utility;
using AncientMemorial.Objects;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class DefaultGetting : Getting {

		// 정적 프로퍼티
		private static readonly int SpawnEffectPrefabId = "SpawnEffect".GetHash();

		// 인스턴스 프로퍼티
		private Color InitialColor;
		private Sprite InitialSprite;
		private UObject SpawnEffect;
		private long SpawnLife;

		// 인스턴스 메서드
		public DefaultGetting(UObject Target) : base(Target) { }

		// 오버라이드 메서드
		protected override void OnStartEffect() {
			SpawnEffect = null;
			if (target.spriteRenderer) {
				InitialColor = target.spriteRenderer.color;
				InitialSprite = target.spriteRenderer.sprite;
				target.spriteRenderer.sprite = target.whiteSpawnSprite ? target.whiteSpawnSprite : InitialSprite;
			}
			target.FreezeAnimator();
			if (UObjectPool.instance && UObjectPool.instance.Contains("SpawnEffect") && GameManager.instance && GameManager.instance.Crystal) {
				SpawnEffect = UObject.Get(SpawnEffectPrefabId, target.transform.position, false,
					Effect => Effect.GetComponent<SpawnEffectHelper>().t_s = duration).GetComponent<UObject>();
				SpawnLife = SpawnEffect.lifeNumber;
			}
			OnEffectRoutine(0);
		}

		protected override void OnEffectRoutine(float DeltaTime) {
			if (!target.spriteRenderer) return;
			float Progress = duration <= 0 ? 1 : Mathf.Clamp01(elapsed / duration);
			Color SpawnColor = GameManager.instance ? GameManager.instance.spawnColor : Color.white;
			target.spriteRenderer.color = new Color(Mathf.Lerp(SpawnColor.r, 1, Progress),
				Mathf.Lerp(SpawnColor.g, 1, Progress), Mathf.Lerp(SpawnColor.b, 1, Progress), Progress);
		}

		protected override void ClearEffect() {
			if (!hasStartedEffect) return;
			if (SpawnEffect && SpawnEffect.lifeNumber == SpawnLife) SpawnEffect.Release(false);
			SpawnEffect = null;
			if (target.spriteRenderer) {
				target.spriteRenderer.color = InitialColor;
				target.spriteRenderer.sprite = isComplete && target.colorSpawnSprite ? target.colorSpawnSprite : InitialSprite;
			}
			target.UnfreezeAnimator();
		}
	}
}
