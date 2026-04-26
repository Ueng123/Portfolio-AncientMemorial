using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.UValues.UNumbers {
	[Serializable]
	public class UObjectDistance : UValue<float> {
		public override bool getIsDynamic => true;

		[SerializeReference][SubclassSelector]
		public UValue<UObject> A;
		
		[SerializeReference][SubclassSelector]
		public UValue<UObject> B;
		
		public override float getValue => Vector2.Distance(A.value.transform.position, B.value.transform.position);
	}
}