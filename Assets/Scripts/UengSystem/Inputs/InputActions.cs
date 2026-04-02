using System;
using UnityEngine.InputSystem;

namespace UengSystem.Inputs {
	[Serializable]
	public struct InputActions {
		public InputAction     inputAction;
		public InputActionType actionType;
		public InputType       inputType;
	}
}