using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UIntToFloat : UValue<float> {
		protected override bool getIsDynamic {
			get {
				intValue.parent = this;
				return intValue.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector]
		public UValue<int> intValue;

		protected override float getValue => intValue.value;
	}
}