using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.UToggles {
	public class UToggle : UUIComponent {

		// 인스턴스 프로퍼티
		[HideInInspector]
		public Toggle toggle;
		
		private bool toggleCache;
		public  bool valueChanged;

		// 인스턴스 메서드
		public virtual void OnValueChanged(bool value) {
			toggleCache = value;
			valueChanged = true;
		}
		
		public bool GetValue() => toggleCache;
		public void SetValue(bool value) {
			if (value == toggleCache) return;
			toggleCache  = value;
			toggle.isOn  = value;
			valueChanged = true;
		}

		// 오버라이드 메서드
		public override void Initialize() {
			toggle      = GetComponent<Toggle>();
			toggleCache = toggle.isOn;
			toggle.onValueChanged.AddListener(OnValueChanged);
		}
		
		public override void Uninitialize() {
			toggle.onValueChanged.RemoveAllListeners();
		}
	}
}