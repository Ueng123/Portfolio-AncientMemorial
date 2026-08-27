using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UObjectCategory : UValue<string> {
		protected override bool   getIsDynamic => false;

		[SerializeReference][SubclassSelector]
		public UValue<UObject> obj;

		protected override string getValue => obj.value.Category;
	}
}