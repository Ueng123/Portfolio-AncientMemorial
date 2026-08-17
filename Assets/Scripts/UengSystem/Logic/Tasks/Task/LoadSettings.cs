using System;
using UengSystem.Settings;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class LoadSettings : TaskComponent{
		public override void Execute(ITaskable self) {
			Setting.Load();
		}
	}
}