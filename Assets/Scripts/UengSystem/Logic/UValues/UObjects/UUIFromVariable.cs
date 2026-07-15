using System;
using AncientMemorial;
using UengSystem.Managers;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UUIFromVariable : UValue<UUI> {
		public override bool getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<string> varID;

		public override UUI getValue => GameManager.UValueUUIVariables[varID.value].value;
	}
}