using System;
using UengSystem.Settings;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UFloatFromSetting : UValue<float> {
		protected override bool        getIsDynamic => true;
		public             SettingType type;
		protected override float       getValue => Setting.GetFloat(type);
	}
}