using System;
using UengSystem.Objects;
using UengSystem.VisualScripting.Tasks;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.UI.UToggles {
	[Serializable]
	public class UToggleAction : UUIAction {

		// 인스턴스 프로퍼티
		private UToggle toggle;
		
		[SerializeReference] [SubclassSelector]
		public UValue<bool> initialValue;

		public Task OnToggleTrue;
		public Task OnToggleFalse;

		// 오버라이드 메서드
		public override void Initialize(UObject   self) {
			toggle = GetComponent<UToggle>();
			toggle.Initialize();
			
			toggle.SetValue(initialValue?.value??false);
			toggle.valueChanged = false;
		}

		public override void Uninitialize(UObject self) {
			component.Uninitialize();
		}

		public override void Routine(UObject      self) {
			if (!toggle.valueChanged) return;
			toggle.valueChanged = false;
			
			if (toggle.GetValue()) {
			OnToggleTrue?.Execute(self);
			}
			else {
			OnToggleFalse?.Execute(self);
			}
		}
	}
}
