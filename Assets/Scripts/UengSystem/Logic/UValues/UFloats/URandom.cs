using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class URandom :UValue<float> {
		public override bool  getIsDynamic => true;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> Min;
		[SerializeReference] [SubclassSelector]
		public UValue<float> Max;
		
		public override float getValue => Random.Range(Min.value, Max.value);
	}
}