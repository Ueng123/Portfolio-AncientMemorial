using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class CompareNumbers : UValue<bool> {
		public override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamic || B.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> A;
		
		[SerializeReference][SubclassSelector]
		public UValue<float> B;

		public CompareType compareType;

		public override bool getValue => compareType switch {
				CompareType.Greater        => A.value >  B.value,
				CompareType.Equal          => Mathf.Approximately(A.value, B.value),
				CompareType.Less           => A.value <  B.value,
				CompareType.GreaterOrEqual => A.value >= B.value,
				CompareType.LessOrEqual    => A.value <= B.value,
				_                          => throw new ArgumentOutOfRangeException()
			};
	}
}