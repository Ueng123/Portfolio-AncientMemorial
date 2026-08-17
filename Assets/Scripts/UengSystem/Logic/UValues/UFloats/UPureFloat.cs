using System;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UPureFloat : UValue<float> {
		protected override bool getIsDynamic => false;
		
		public             float number;
		protected override float getValue => number;
	}
}