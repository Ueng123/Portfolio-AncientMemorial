using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UObjectExists : UValue<bool> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> id;

		public override bool getValue => Objects.UObject.UObjectExists(id.value);
	}
}