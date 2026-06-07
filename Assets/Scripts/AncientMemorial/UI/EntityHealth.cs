using System;
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
			slider.fillRect.GetComponent<Image>().color    = new Color(1f,         0.4103774f, 0.4103774f, 1f);
			background.color                               = new Color(0.3113208f, 0.113074f,  0.113074f,  1f);
			subSlider.fillRect.GetComponent<Image>().color = Color.white;
		}

		public override void SetValue(float value) {
			slider.value    = value;
			subSlider.value = Mathf.Lerp(subSlider.value, value, (value==0?25f:5f)*Time.deltaTime);
		}
	}
}