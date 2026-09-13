using System;
using UengSystem.SaveDatas.SettingDatas;

namespace UengSystem.VisualScripting.UValues.UBools {
	[Serializable]
	public class UBoolFromSetting
		: UValue<bool> {

		// 인스턴스 프로퍼티
		protected override bool        getIsDynamic => true;
		public             SettingType type;
		protected override bool        getValue => Setting.GetBool(type);
	}
}