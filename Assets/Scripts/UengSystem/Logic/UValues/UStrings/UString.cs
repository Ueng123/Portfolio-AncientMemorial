using System;
using System.Linq;
using UnityEngine;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UString : UValue<string> {
		public override bool getIsDynamic {
			get {
				bool result = false;
				
				foreach (UValue<string> str in strings) {
					str.parent = this;
					result     = result || str.getIsDynamic;
				}

				return result;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string>[] strings;
		
		public override string getValue => strings.Aggregate("", (current, item) => current + item.value);
	}
}