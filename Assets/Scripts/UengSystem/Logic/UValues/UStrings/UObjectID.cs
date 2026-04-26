using System;
using UengSystem.Objects;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UObjectID : UValue<string> {
		public override bool   getIsDynamic => false;

		public UObject obj;

		public override string getValue => obj.ID;
	}
}