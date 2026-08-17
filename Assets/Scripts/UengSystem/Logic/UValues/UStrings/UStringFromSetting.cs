using System;
using UengSystem.Settings;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UStringFromSetting : UValue<string> {
		protected override bool        getIsDynamic => true;
		public             SettingType type;
		protected override string      getValue => Setting.GetString(type);
	}
}