using System;
using AncientMemorial;
using UnityEngine;

namespace UengSystem.Logic.UValues.UColors {
	[Serializable]
	public class UColorFromVariable : UValue<Color> {
		protected override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		protected override Color getValue => GameManager.UValueColorVariables[varID.value].value;
	}
}