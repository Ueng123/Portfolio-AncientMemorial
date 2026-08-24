using System;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class StopGame : TaskComponent {
		public override void Execute(ITaskable self) {
			GameManager.SetTimeScale(0);
		}
	}
}