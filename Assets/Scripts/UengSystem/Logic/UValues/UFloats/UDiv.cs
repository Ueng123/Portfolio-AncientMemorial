using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UDiv : UValue<float> {
		public override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamicAuto || B.isDynamicAuto;
			}
		}
		
		[Header("A / B")] 
		[SerializeReference][SubclassSelector] public UValue<float> A;
		[SerializeReference][SubclassSelector] public UValue<float> B;

		public override float getValue => A.value / B.value;
	}
}