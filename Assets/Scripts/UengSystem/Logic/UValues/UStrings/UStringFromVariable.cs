using System;
using AncientMemorial;
using UnityEngine;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UStringFromVariable : UValue<string> {
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		protected override string getValue => GameManager.UValueStringVariables[varID.value].value;
	}
}