using UnityEngine;
using UnityEngine.UI;

namespace UengSystem.UI.UToggles {
	public class UToggle : UUIComponent {
		[HideInInspector]
		public Toggle toggle;
		
		private bool toggleCache;
		public  bool valueChanged;
		
		public override void Initialize() {
			toggle      = GetComponent<Toggle>();
			toggleCache = toggle.isOn;
			toggle.onValueChanged.AddListener(OnValueChanged);
		}
		
		public override void Uninitialize() {
			toggle.onValueChanged.RemoveAllListeners();
		}

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
	}
}