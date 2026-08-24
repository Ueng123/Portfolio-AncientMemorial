using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.USliders {
	public class EntityHealth : USlider {
		public Slider subSlider;
		public Image  background;
		
		public override void   Initialize() {
			base.Initialize();

			slider.value                                   = 1;
			subSlider.value                                = 1;
		}

		public override void SetValue(float value) {
			slider.value    = value;
			subSlider.value = Mathf.Lerp(subSlider.value, value, (value==0?25f:5f)*Time.deltaTime);
		}
	}
}