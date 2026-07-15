using System;
using UengSystem.Settings;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class SettingValueS : UValue<string> {
		public override bool        getIsDynamic => true;
		public          SettingType type;
		public override string      getValue => Setting.GetValueString(type);
	}
}