using System;
using AncientMemorial;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ResumeGame : TaskComponent {
		public override void Execute(ITaskable self) {
			GameManager.SetTimeScale(1);
		}
	}
}