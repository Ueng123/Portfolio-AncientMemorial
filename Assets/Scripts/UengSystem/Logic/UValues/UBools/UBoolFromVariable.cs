using System;
using JetBrains.Annotations;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class UBoolFromVariable : UValue<bool> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		public override bool getValue => GameManager.UValueBoolVariables[varID.value].value;
	}
}