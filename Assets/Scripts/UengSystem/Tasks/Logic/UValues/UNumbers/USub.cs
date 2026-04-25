using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UNumbers {
	[Serializable]
	public class USub : UValue<float> {
		public override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamic || B.isDynamic;
			}
		}

		[Header("A - B")] 
		[SerializeReference][SubclassSelector] public UValue<float> A;
		[SerializeReference][SubclassSelector] public UValue<float> B;

		public override float getValue => A.value - B.value;
	}
}