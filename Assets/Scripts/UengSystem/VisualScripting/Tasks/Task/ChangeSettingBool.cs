using System;
using UengSystem.SaveDatas.SettingDatas;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	// 나중에안쓰면삭제요망
	[Serializable]
	public class ChangeSettingBool : TaskComponent {

		public SettingType   settingType;
		
		[SerializeReference] [SubclassSelector]
		public UValue<bool> value;
		
		public override void Execute(ITaskable self) {
			Setting.SetValue(settingType, value.value);
		}
	}
}