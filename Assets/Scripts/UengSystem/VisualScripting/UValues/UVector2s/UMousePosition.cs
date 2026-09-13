using System;
using UengSystem.Inputs;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UVector2s {
	[Serializable]
	public class UMousePosition : UValue<Vector2> {

		// 인스턴스 프로퍼티
		protected override bool    getIsDynamic => true;
		protected override Vector2 getValue     => InputManager.mousePosition;
	}
}