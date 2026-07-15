using System;
using UengSystem.Settings;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class SettingValueF : UValue<float> {
		public override bool        getIsDynamic => true;
		public          SettingType type;
		public override float       getValue => Setting.GetValueFloat(type);
	}
}