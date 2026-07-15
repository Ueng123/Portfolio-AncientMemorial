using System;
using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USlider : UUIComponent {
		[HideInInspector]
		public Slider slider;
		protected Image sliderFillImage;

		public bool valueChanged = false;

		private float _sliderValue;
		private float sliderValue {
			set {
				if (Mathf.Approximately(_sliderValue, value)) return;
				valueChanged = true;
				_sliderValue = value;
			}
		}

		public override void Initialize() {
			slider          = GetComponent<Slider>();
			sliderFillImage = slider.fillRect?.GetComponent<Image>();
		}
		
		public virtual float GetValue() {
			sliderValue  = slider.value;
			return slider.value;
		}

		public virtual void SetValue(float value) {
			slider.value = value;
			sliderValue  = value;
		}

		public Color GetColor()            => sliderFillImage.color;
		public void  SetColor(Color color) => sliderFillImage.color = color;
	}
}