using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UNormalized : UValue<Vector2> {
		protected override bool getIsDynamic {
			get {
				vector.parent =  this;
				return vector.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector] public UValue<Vector2> vector;

		protected override Vector2 getValue => vector.value.normalized;
	}
}