using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UBools {
	[Serializable]
	public class CompareNumbers : UValue<bool> {
		public override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.getIsDynamic || B.getIsDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> A;
		
		[SerializeReference][SubclassSelector]
		public UValue<float> B;

		public CompareType compareType;

		public override bool getValue => compareType switch {
				CompareType.Greater        => A.getValue >  B.getValue,
				CompareType.Equal          => Mathf.Approximately(A.getValue, B.getValue),
				CompareType.Less           => A.getValue <  B.getValue,
				CompareType.GreaterOrEqual => A.getValue >= B.getValue,
				CompareType.LessOrEqual    => A.getValue <= B.getValue,
				_                          => throw new ArgumentOutOfRangeException()
			};
	}
}