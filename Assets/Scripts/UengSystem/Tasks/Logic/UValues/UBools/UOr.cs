using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UBools {
	[Serializable]
	public class UOr : UValue<bool> {
		public override bool getIsDynamic => A.getIsDynamic || B.getIsDynamic;
		
		[Header("A || B")]
		[SerializeReference] [SubclassSelector] public UValue<bool> A;
		[SerializeReference] [SubclassSelector] public UValue<bool> B;

		public override bool getValue => A.getValue || B.getValue;
	}
}