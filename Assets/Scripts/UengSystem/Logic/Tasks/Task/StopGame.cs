using System;
using AncientMemorial;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class StopGame : TaskComponent {
		public override void Execute(ITaskable self) {
			GameManager.SetTimeScale(0);
		}
	}
}