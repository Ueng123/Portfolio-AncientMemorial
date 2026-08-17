using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UMagnitude : UValue<float> {
		protected override bool getIsDynamic {
			get {
				vector.parent = this;
				return vector.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector] public UValue<Vector2> vector;

		protected override float getValue => vector.value.magnitude;
	}
}