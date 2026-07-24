using System;
using UengSystem.Objects;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UObjectCategory : UValue<string> {
		public override bool   getIsDynamic => false;

		public Objects.UObject obj;

		public override string getValue => obj.Category;
	}
}