using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UVecDiv : UValue<Vector2> {
		public override bool getIsDynamic {
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamicAuto || B.isDynamicAuto;
			}
		}
		
		[Header("A(vec) / B")]
		[SerializeReference][SubclassSelector] public UValue<Vector2> A;
		[SerializeReference][SubclassSelector] public UValue<float> B;

		public override Vector2 getValue => A.value / B.value;
	}
}