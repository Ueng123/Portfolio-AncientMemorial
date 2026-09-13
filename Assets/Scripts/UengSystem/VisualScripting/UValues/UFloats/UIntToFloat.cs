using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UIntToFloat : UValue<float> {

		// 인스턴스 프로퍼티
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