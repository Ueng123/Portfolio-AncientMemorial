using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class URandomInt :UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool  getIsDynamic => true;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> Min;
		[SerializeReference] [SubclassSelector]
		public UValue<float> Max;

		protected override float getValue => Random.Range((int)Min.value, (int)Max.value);
	}
}