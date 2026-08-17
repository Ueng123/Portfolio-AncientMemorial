using System;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UPureString : UValue<string> {
		protected override bool getIsDynamic => false;
		
		public             string Text;
		protected override string getValue => Text;
	}
}