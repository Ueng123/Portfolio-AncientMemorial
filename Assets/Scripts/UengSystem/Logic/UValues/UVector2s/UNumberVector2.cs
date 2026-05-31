using UnityEngine;

namespace UengSystem.Logic.UValues.UVector2s {
	public class UNumberVector2 : UValue<Vector2> {
		public override bool getIsDynamic {
			get {
				x.parent = this;
				y.parent = this;
				return x.isDynamic || y.isDynamic;
			}
		}

		[SerializeReference][SubclassSelector] public UValue<float> x;
		[SerializeReference][SubclassSelector] public UValue<float> y;

		public override Vector2 getValue => new (x.value, y.value);
	}
}