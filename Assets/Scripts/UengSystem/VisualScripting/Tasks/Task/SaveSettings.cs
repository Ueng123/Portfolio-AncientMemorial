using System;
using UengSystem.SaveDatas.SettingDatas;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SaveSettings : TaskComponent {
		public override void Execute(ITaskable self) {
			Setting.Save();
		}
	}
}