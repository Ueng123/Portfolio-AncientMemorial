using System;
using UengSystem.SaveDatas.SettingDatas;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UStringFromSetting : UValue<string> {

		// 인스턴스 프로퍼티
		protected override bool        getIsDynamic => true;
		public             SettingType type;
		protected override string      getValue => Setting.GetString(type);
	}
}