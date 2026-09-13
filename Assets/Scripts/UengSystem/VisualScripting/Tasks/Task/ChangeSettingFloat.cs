using System;
using UengSystem.SaveDatas.SettingDatas;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ChangeSettingFloat : TaskComponent {

		// 인스턴스 프로퍼티
		public SettingType   settingType;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> number;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			Setting.SetValue(settingType, number.value);
		}
	}
}