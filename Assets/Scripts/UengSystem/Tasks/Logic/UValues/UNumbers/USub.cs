using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UNumbers {
	[Serializable]
	public class USub : UValue<float> {
		public override bool getIsDynamic => A.getIsDynamic || B.getIsDynamic;

		[Header("A - B")] 
		[SerializeReference][SubclassSelector] public UValue<float> A;
		[SerializeReference][SubclassSelector] public UValue<float> B;

		public override float getValue => A.getValue - B.getValue;
	}
}