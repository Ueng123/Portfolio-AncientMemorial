using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	[Serializable]
	public class UNumberVector2 : UValue<Vector2> {
		protected override bool getIsDynamic {
			get {
				x.parent = this;
				y.parent = this;
				return x.isDynamic || y.isDynamic;
			}
		}

		[SerializeReference][SubclassSelector] public UValue<float> x;
		[SerializeReference][SubclassSelector] public UValue<float> y;

		protected override Vector2 getValue => new (x.value, y.value);
	}
}