using System;
using UengSystem.Inputs;
using UengSystem.Tasks;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButtonAction : UUIAction {
		public UButton         button;
		public InputActionType actionType;
		public InputPressType  pressType;
		public Task            task;

		public override void Routine(ITaskable self) {
			if (!button.clicked && InputManager.inputData[actionType].pressType != pressType) return;
			
			button.clicked = false;
			task.Execute(self);
		}
	}
}