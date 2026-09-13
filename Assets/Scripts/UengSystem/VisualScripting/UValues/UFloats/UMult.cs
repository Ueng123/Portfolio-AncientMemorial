using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UMult : UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamic || B.isDynamic;
			}
		}
		
		[Header("A * B")] 
		[SerializeReference][SubclassSelector] public UValue<float> A;
		[SerializeReference][SubclassSelector] public UValue<float> B;

		protected override float getValue => A.value * B.value;
	}
}