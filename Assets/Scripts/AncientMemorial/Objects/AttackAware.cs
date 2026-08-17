using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class AttackAware : UObject {
		public SpriteRenderer whiteObject;
		public Vector2    targetSize;

		public Color initialColor;
		public Color targetColor;

		public bool lerpX = true;
		public bool lerpY = true;
		
		private StopWatch stopWatch;
		public float duration;
		
		protected override void Routine() {
			whiteObject.size     = new Vector2(targetSize.x * Mathf.Clamp01(lerpX&&duration !=0 ? stopWatch.Tock() / duration : 1),
											   targetSize.y * Mathf.Clamp01(lerpY&&duration !=0 ? stopWatch.Tock() / duration : 1));
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * (1 /duration) * 5f);
			whiteObject.color    = spriteRenderer.color;
		}
		
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