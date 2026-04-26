using System;

namespace UengSystem.Logic.UValues.UNumbers {
	[Serializable]
	public class UPureNumber : UValue<float> {
		public override bool getIsDynamic => false;
		
		public          float number;
		public override float getValue => number;
	}
}