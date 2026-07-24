using System;
using UengSystem.Objects;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UObjectsExist : UValue<bool> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> category;

		public override bool getValue => Objects.UObject.UObjectsExist(category.value);
	}
}