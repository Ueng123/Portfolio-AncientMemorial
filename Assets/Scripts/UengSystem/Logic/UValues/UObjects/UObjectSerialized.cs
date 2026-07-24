using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UObjectSerialized : UValue<Objects.UObject> {
		public override bool getIsDynamic => false;

		public Objects.UObject obj;

		public override Objects.UObject getValue => obj;
	}
}