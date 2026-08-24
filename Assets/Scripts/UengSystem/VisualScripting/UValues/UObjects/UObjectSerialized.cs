using System;
using UengSystem.Objects;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UObjectSerialized : UValue<UObject> {
		protected override bool getIsDynamic => false;

		public UObject obj;

		protected override UObject getValue => obj;
	}
}