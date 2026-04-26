using System;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UStringFromVariable : UValue<string> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		public override string getValue => GameManager.UValueStringVariables[varID.value].value;
	}
}