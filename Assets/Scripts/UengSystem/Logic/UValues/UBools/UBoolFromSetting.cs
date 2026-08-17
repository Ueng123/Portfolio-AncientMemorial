using System;
using UengSystem.Settings;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UBoolFromSetting
		: UValue<bool> {
		protected override bool        getIsDynamic => true;
		public             SettingType type;
		protected override bool        getValue => Setting.GetBool(type);
	}
}