using System;
using UengSystem.Logic.UValues;
using UengSystem.Settings;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	// 나중에안쓰면삭제요망
	[Serializable]
	public class ChangeSettingString : TaskComponent {

		public SettingType   settingType;
		
		[SerializeReference] [SubclassSelector]
		public UValue<string> text;
		
		public override void Execute(ITaskable self) {
			Setting.SetValueString(settingType, text.value);
		}
	}
}