using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class AttackAware : AdvancedUObject {
		public SpriteRenderer whiteObject;
		public Vector2    targetSize;

		public Color initialColor;
		public Color targetColor;

		public bool lerpX = true;
		public bool lerpY = true;
		
		private StopWatch stopWatch;
		public float duration;
		
		protected override void EarlyRoutine() { }

		protected override void Routine() {
			whiteObject.size     = new Vector2(targetSize.x * (lerpX ? stopWatch.Tock() / duration : 1),
											   targetSize.y * (lerpY ? stopWatch.Tock() / duration : 1));
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * (1 /duration) * 5f);
			whiteObject.color    = spriteRenderer.color;
		}

		protected override void LateRoutine() { }

		protected override void FixedRoutine() { }

		public override void Initialize() {
			base.Initialize();
			stopWatch = new StopWatch();
			stopWatch.Tick();
			
			whiteObject.size     = Vector2.zero;
			spriteRenderer.size  = targetSize;
			
			spriteRenderer.color    = initialColor;
			whiteObject.color    = initialColor;
		}

		public override void Uninitialize() { }
	}
}