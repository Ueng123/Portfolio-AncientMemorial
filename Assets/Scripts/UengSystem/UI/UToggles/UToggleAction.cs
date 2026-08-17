using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.UI.UToggles {
	[Serializable]
	public class UToggleAction : UUIAction {
		private UToggle toggle;
		
		[SerializeReference] [SubclassSelector]
		public UValue<bool> initialValue;

		public Task OnToggleTrue;
		public Task OnToggleFalse;
		
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