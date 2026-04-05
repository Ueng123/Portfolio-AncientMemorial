using System;
using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USlider : UUIComponent {
		private Slider slider;

		public override void Initialize() {
			slider = GetComponent<Slider>();
		}
		
		public float GetValue() => slider.value;
		public void SetValue(float value) => slider.value = value;
	}
}