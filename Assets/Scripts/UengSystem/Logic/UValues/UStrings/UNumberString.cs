using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UNumberString : UValue<string> {
		public override bool getIsDynamic {
			get {
				number.parent = this;
				return number.isDynamic;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<float> number;
		
		public override string getValue => number.getValue.ToString();
	}
}