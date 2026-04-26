using System;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UPureString : UValue<string> {
		public override bool getIsDynamic => false;
		
		public          string Text;
		public override string getValue => Text;
	}
}