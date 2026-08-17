using System;
using AncientMemorial;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UObjectFromVariable : UValue<Objects.UObject> {
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		protected override UObject getValue => GameManager.UValueUObjectVariables[varID.value].value;
	}
}