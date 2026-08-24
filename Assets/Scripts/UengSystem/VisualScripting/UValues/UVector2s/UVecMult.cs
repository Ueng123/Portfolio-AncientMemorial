using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UVector2s {
	[Serializable]
	public class UVecMult : UValue<Vector2> {
		protected override bool getIsDynamic {
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamic || B.isDynamic;
			}
		}
		
		[Header("A(vec) * B")]
		[SerializeReference][SubclassSelector] public UValue<Vector2> A;
		[SerializeReference][SubclassSelector] public UValue<float> B;

		protected override Vector2 getValue => A.value * B.value;
	}
}