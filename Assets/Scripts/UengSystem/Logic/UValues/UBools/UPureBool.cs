using System;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UPureBool : UValue<bool> {
		protected override bool getIsDynamic => false;

		public bool boolValue;

		protected override bool getValue     => boolValue;
	}
}