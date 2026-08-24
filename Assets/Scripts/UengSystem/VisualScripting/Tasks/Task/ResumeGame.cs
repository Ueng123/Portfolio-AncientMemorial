using System;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ResumeGame : TaskComponent {
		public override void Execute(ITaskable self) {
			GameManager.SetTimeScale(1);
		}
	}
}