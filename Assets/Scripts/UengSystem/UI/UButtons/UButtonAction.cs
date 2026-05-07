using System;
using UengSystem.Inputs;
using UengSystem.Logic.Tasks;

namespace UengSystem.UI.UButtons {
	[Serializable]
	public class UButtonAction : UUIAction {
		public UButton         button;
		public InputActionType actionType;
		public InputPressType  pressType;
		public Task            task;

		public override void Initialize(ITaskable self) {
			component?.Initialize();
		}

		private bool doTask = false;
		public override void Routine(ITaskable self) {
			doTask = InputManager.inputData[actionType].pressType == pressType;
			
			if (button) {
				doTask         = doTask || button.clicked;
				button.clicked = false;
			}

			if (!doTask) return;
			
			task.Execute(self);
		}
	}
}