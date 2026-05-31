using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UObjectSerialized : UValue<UObject> {
		public override bool getIsDynamic => false;

		public UObject obj;

		public override UObject getValue => obj;
	}
}