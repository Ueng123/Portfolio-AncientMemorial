using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UArcTrigFunc : UValue<float> {
		protected override bool getIsDynamic {
			get {
				vector.parent = this;
				return vector.isDynamic;
			}
		}

		public TrigFunction    funcType;
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> vector;

		public bool exchangeToDeg;

		protected override float getValue => (exchangeToDeg ? Mathf.Rad2Deg : 1) * funcType switch {
			TrigFunction.sin => Mathf.Asin (vector.value.y / vector.value.magnitude),
			TrigFunction.cos => Mathf.Acos (vector.value.x / vector.value.magnitude),
			TrigFunction.tan => Mathf.Atan2(vector.value.y, vector.value.x),
			TrigFunction.csc => Mathf.Asin (vector.value.magnitude / vector.value.y),
			TrigFunction.sec => Mathf.Acos (vector.value.magnitude / vector.value.x),
			TrigFunction.cot => Mathf.Atan2(vector.value.x, vector.value.y),
			_                => throw new ArgumentOutOfRangeException()
		};
	}
}