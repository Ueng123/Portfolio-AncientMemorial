using System;
using UengSystem.Tasks.Logic.Value;
using UnityEngine;

namespace UengSystem.Tasks.Logic.String {
	[Serializable]
	public class UNumberString : UStringComponent {
		[SerializeReference][SubclassSelector]
		public UNumber number;
		
		public override string GetText() {
			return number.GetValue().ToString();
		}
	}
}