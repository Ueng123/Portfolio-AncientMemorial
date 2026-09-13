using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UObjectDistance : UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic => true;

		[SerializeReference][SubclassSelector]
		public UValue<Objects.UObject> A;
		
		[SerializeReference][SubclassSelector]
		public UValue<Objects.UObject> B;

		protected override float getValue => Vector2.Distance(A.value.transform.position, B.value.transform.position);
	}
}