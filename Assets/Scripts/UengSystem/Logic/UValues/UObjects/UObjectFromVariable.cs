using System;
using AncientMemorial;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UObjectFromVariable : UValue<UObject> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		public override UObject getValue => GameManager.UValueUObjectVariables[varID.value].value;
	}
}