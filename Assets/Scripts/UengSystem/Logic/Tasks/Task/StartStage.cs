using System;
using UengSystem.Managers;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class StartStage : TaskComponent {
		public override void Execute(ITaskable self) {
			USceneManager.StartStage();
		}
	}
}