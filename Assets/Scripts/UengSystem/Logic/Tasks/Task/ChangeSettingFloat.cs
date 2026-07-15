using System;
using UengSystem.Logic.UValues;
using UengSystem.Settings;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ChangeSettingFloat : TaskComponent {

		public SettingType   settingType;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> number;
		
		public override void Execute(ITaskable self) {
			Setting.SetValueFloat(settingType, number.value);
		}
	}
}