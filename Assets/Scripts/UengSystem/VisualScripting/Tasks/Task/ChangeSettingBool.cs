using System;
using UengSystem.SaveDatas.SettingDatas;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	// 나중에안쓰면삭제요망
	[Serializable]
	public class ChangeSettingBool : TaskComponent {

		// 인스턴스 프로퍼티
		public SettingType   settingType;
		
		[SerializeReference] [SubclassSelector]
		public UValue<bool> value;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			Setting.SetValue(settingType, value.value);
		}
	}
}