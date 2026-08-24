using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UNumberString : UValue<string> {
		protected override bool getIsDynamic {
			get {
				number.parent = this;
				return number.isDynamic;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<float> number;

		protected override string getValue => number.value.ToString();
	}
}