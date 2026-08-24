using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class URandom :UValue<float> {
		protected override bool  getIsDynamic => true;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> Min;
		[SerializeReference] [SubclassSelector]
		public UValue<float> Max;

		protected override float getValue => Random.Range(Min.value, Max.value);
	}
}