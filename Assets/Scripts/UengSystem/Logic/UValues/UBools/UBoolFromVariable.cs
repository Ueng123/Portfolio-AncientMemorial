using System;
using AncientMemorial;
using UnityEngine;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UBoolFromVariable : UValue<bool> {
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		protected override bool getValue => GameManager.UValueBoolVariables[varID.value].value;
	}
}