using System;
using UnityEngine.InputSystem;

namespace AncientMemorial.Inputs {
	[Serializable]
	public struct InputActions {
		public InputAction     inputAction;
		public InputActionType actionType;
		public InputType       inputType;
	}
}