using System;
using AncientMemorial;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UFloatFromVariable : UValue<float> {
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		protected override float getValue => GameManager.UValueFloatVariables[varID.value].value;
	}
}