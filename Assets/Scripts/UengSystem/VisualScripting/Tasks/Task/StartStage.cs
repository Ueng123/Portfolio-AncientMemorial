using System;
using UengSystem.StageManager;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class StartStage : TaskComponent {
		public override void Execute(ITaskable self) {
			USceneManager.StartStage();
		}
	}
}