using System;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.UValues.UColors {
	[Serializable]
	public class UUIFromVariable : UValue<UUI> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		public override UUI getValue => GameManager.UValueUUIVariables[varID.value].value;
	}
}