using System;
using AncientMemorial;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.UValues.UColors {
	[Serializable]
	public class UColorFromVariable : UValue<Color> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		public override Color getValue => GameManager.UValueColorVariables[varID.value].value;
	}
}