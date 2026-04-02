using System;
using UengSystem.Tasks.Logic.Value;
using UnityEngine;

namespace UengSystem.Tasks.Logic {
	public class CompareValues : UCondition {
		[SerializeReference] [SubclassSelector]
		public UNumber A;
		
		[SerializeReference][SubclassSelector]
		public UNumber B;

		public CompareType compareType;
		
		public override bool Check() {
			return compareType switch {
				CompareType.Greater        => A.GetValue() > B.GetValue(),
				CompareType.Equal          => Mathf.Approximately(A.GetValue(), B.GetValue()),
				CompareType.Less           => A.GetValue() < B.GetValue(),
				CompareType.GreaterOrEqual => A.GetValue() >= B.GetValue(),
				CompareType.LessOrEqual    => A.GetValue() <= B.GetValue(),
				_                          => throw new ArgumentOutOfRangeException()
			};
		}
	}
}