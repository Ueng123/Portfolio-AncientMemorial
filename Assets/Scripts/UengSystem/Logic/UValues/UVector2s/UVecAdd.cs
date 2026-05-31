using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UVecAdd : UValue<Vector2> {
		public override bool getIsDynamic {
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamicAuto || B.isDynamicAuto;
			}
		}
		
		[Header("A + B")]
		[SerializeReference][SubclassSelector] public UValue<Vector2> A;
		[SerializeReference][SubclassSelector] public UValue<Vector2> B;

		public override Vector2 getValue => A.value + B.value;
	}
}