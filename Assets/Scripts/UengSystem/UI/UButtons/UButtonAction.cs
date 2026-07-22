using System;
using UengSystem.Inputs;
using UengSystem.Logic.Tasks;
using UengSystem.Objects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButtonAction : UUIAction {
		public InputActionType actionType;
		public InputPressType  pressType;
		public Task            taskOnClicked;

		private UButton button;
		private Button  buttonObject;

		public override void Initialize(UObject self) {
			if (!component) return;
			component.Initialize();
			button       = (UButton)component;
			buttonObject = button.button;
		}
		
		public override void Routine(UObject self) {
			if (component) {
				bool input = InputManager.inputData[actionType].pressType == pressType;
                bool buttonClickable = buttonObject.enabled && buttonObject.interactable;
                if (input&&buttonClickable) button.OnButtonClicked();
				
                if (!button.clicked) return;
                button.clicked = false;
				
                taskOnClicked.Execute(self);
			}
			else {
				bool input = InputManager.inputData[actionType].pressType == pressType;
				if (input) taskOnClicked.Execute(self);
			}
		}
	}
}