using System;
using UnityEngine.InputSystem;

namespace UengSystem.Inputs {
	[Serializable]
	public struct InputActions {

		// 인스턴스 프로퍼티
		public InputAction inputAction;
		public ActionType  actionType;
		public InputType   inputType;
	}
}