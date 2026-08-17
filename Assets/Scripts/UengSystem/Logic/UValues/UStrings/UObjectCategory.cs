using System;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UObjectCategory : UValue<string> {
		protected override bool   getIsDynamic => false;

		public Objects.UObject obj;

		protected override string getValue => obj.Category;
	}
}