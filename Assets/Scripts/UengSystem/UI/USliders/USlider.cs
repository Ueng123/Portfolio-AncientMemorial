using System;
using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USlider : UUIComponent {

		// 인스턴스 프로퍼티
		[HideInInspector]
		public Slider slider;
		protected Image sliderFillImage;

		private float valueCache;
		public bool valueChanged = false;

		// 인스턴스 메서드
		public virtual void OnValueChanged(float value) {
			valueCache   = value;
			valueChanged = true;
		}

		public virtual float GetValue() => valueCache;
		
		public virtual void SetValue(float value) {
			if (Mathf.Approximately(value, valueCache)) return;
			valueCache   = value;
			slider.value = value;
		}

		public Color GetColor()            => sliderFillImage.color;
		public void  SetColor(Color color) => sliderFillImage.color = color;

		// 오버라이드 메서드
		public override void Initialize() {
			slider          = GetComponent<Slider>();
			valueCache      = slider.value;
			sliderFillImage = slider.fillRect?.GetComponent<Image>();
			slider.onValueChanged.AddListener(OnValueChanged);
		}
		
		public override void Uninitialize() {
			slider.onValueChanged.RemoveAllListeners();
		}
	}
}