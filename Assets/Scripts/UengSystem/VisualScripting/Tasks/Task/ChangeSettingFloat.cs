using System;
using UengSystem.SaveDatas.SettingDatas;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ChangeSettingFloat : TaskComponent {

		public SettingType   settingType;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> number;
		
		public override void Execute(ITaskable self) {
			Setting.SetValue(settingType, number.value);
		}
	}
}