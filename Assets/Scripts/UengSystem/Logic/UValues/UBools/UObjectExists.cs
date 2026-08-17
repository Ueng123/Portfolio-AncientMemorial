using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UObjectExists : UValue<bool> {
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> id;

		protected override bool getValue => Objects.UObject.UObjectExists(id.value);
	}
}