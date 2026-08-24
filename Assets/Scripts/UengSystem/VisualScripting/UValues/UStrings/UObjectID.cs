using System;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UObjectID : UValue<string> {
		protected override bool   getIsDynamic => false;

		public Objects.UObject obj;

		protected override string getValue => obj.ID;
	}
}