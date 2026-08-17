using System;
using UengSystem.Settings;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SaveSettings : TaskComponent {
		public override void Execute(ITaskable self) {
			Setting.Save();
		}
	}
}