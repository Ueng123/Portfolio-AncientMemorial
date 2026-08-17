using System;
using UengSystem.Managers;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class LoadMainMenu : TaskComponent {
		public override void Execute(ITaskable self) {
			USceneManager.LoadMainMenu();
		}
	}
}