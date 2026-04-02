using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.Value {
	[Serializable]
	public class UAdd : UNumber {
		[Header("A + B")] 
		[SerializeReference][SubclassSelector] public UNumber A;
		[SerializeReference][SubclassSelector] public UNumber B;

		public override float GetValue() {
			return A.GetValue() + B.GetValue();
		}
	}
}