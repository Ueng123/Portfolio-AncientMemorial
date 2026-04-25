using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UStrings {
	[Serializable]
	public class UNumberString : UValue<string> {
		public override bool getIsDynamic => number.getIsDynamic;
		
		[SerializeReference][SubclassSelector]
		public UValue<float> number;
		
		public override string getValue => number.getValue.ToString();
	}
}