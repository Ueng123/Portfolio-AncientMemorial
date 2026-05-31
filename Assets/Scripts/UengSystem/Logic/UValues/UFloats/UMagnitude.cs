using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UMagnitude : UValue<float> {
		public override bool getIsDynamic {
			get {
				vector.parent = this;
				return vector.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector] public UValue<Vector2> vector;

		public override float getValue => vector.value.magnitude;
	}
}