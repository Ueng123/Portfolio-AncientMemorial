using System;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UPureBool : UValue<bool> {
		public override bool getIsDynamic => false;

		public bool boolValue;
		
		public override bool getValue     => boolValue;
	}
}