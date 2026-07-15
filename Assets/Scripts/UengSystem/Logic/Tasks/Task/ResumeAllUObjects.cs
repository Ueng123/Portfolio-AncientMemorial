using System;
using UengSystem.Objects;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ResumeAllUObjects : TaskComponent {
		public override void Execute(ITaskable self) {
			foreach (UObject obj in UObject.Instances) { obj.Stop(); }
		}
	}
}