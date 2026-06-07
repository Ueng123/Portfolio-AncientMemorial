using System;
using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USlider : UUIComponent {
		protected Slider slider;
		protected Image sliderFillImage;

		public override void Initialize() {
			slider          = GetComponent<Slider>();
			sliderFillImage = slider.fillRect.GetComponent<Image>();
		}
		
		public float GetValue()            => slider.value;
		public virtual void  SetValue(float value) => slider.value = value;

		public Color GetColor()            => sliderFillImage.color;
		public void  SetColor(Color color) => sliderFillImage.color = color;
	}
}