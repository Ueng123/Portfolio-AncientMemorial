using System;
using UengSystem.SaveDatas.SettingDatas;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SaveSettings : TaskComponent {

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			Setting.Save();
		}
	}
}