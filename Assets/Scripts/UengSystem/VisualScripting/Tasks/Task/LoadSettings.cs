using System;
using UengSystem.SaveDatas.SettingDatas;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class LoadSettings : TaskComponent{
		public override void Execute(ITaskable self) {
			Setting.Load();
		}
	}
}