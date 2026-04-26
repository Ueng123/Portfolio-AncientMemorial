using System;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.UValues.UColors {
	[Serializable]
	public class UFloatFromVariable : UValue<float> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		public override float getValue => GameManager.UValueFloatVariables[varID.value].value;
	}
}