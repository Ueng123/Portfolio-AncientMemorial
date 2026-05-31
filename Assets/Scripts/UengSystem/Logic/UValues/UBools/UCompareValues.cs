using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Logic.UValues.UBools {
	[Serializable]
	public class CompareNumbers : UValue<bool> {
		public override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamicAuto || B.isDynamicAuto;
			}
		}
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> A;
		
		[SerializeReference][SubclassSelector]
		public UValue<float> B;

		[FormerlySerializedAs("compareType")] public UCompareType uCompareType;

		public override bool getValue => uCompareType switch {
				UCompareType.Greater        => A.value >  B.value,
				UCompareType.Equal          => Mathf.Approximately(A.value, B.value),
				UCompareType.Less           => A.value <  B.value,
				UCompareType.GreaterOrEqual => A.value >= B.value,
				UCompareType.LessOrEqual    => A.value <= B.value,
				_                          => throw new ArgumentOutOfRangeException()
			};
	}
}