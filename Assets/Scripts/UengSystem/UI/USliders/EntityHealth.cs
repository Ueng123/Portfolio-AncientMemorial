using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.USliders {
	public class EntityHealth : USlider {

		// 인스턴스 프로퍼티
		public Slider subSlider;
		public Image  background;

		// 오버라이드 메서드
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