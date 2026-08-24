using System;
using UengSystem.StageManager;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class LoadMainMenu : TaskComponent {
		public override void Execute(ITaskable self) {
			USceneManager.LoadMainMenu();
		}
	}
}