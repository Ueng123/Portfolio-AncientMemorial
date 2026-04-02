using System;
using UengSystem.Inputs;
using UengSystem.Tasks;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButtonAction : UIAction {
		public UButton         button;
		public InputActionType actionType;
		public Task            task;

		public override void Routine(ITaskable self) {
			if (!button.clicked && InputManager.inputData[actionType].pressType != InputPressType.Down) return;
			
			button.clicked = false;
			task.Execute(self);
		}
	}
}