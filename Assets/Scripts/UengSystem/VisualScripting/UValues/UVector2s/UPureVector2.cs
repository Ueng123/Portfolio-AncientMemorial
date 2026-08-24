using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UVector2s {
	[Serializable]
	public class UPureVector2 : UPureValue<Vector2, UPureVector2> {
		// protected override bool    getIsDynamic => false;

		// [FormerlySerializedAs("oldValue")] [FormerlySerializedAs("vector2")] public Vector2 pureValue;

		// public override void OnAfterDeserialize() {
		// 	base.OnAfterDeserialize();
		// 	pureValue = oldValue;
		// }
		
		// protected override Vector2 getValue => pureValue; 
	}
}