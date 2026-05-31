using System;
using UengSystem.Inputs;
using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UMousePosition : UValue<Vector2> {
		public override bool    getIsDynamic => true;
		public override Vector2 getValue     => InputManager.inputData[InputActionType.MousePosition].valueV;
	}
}