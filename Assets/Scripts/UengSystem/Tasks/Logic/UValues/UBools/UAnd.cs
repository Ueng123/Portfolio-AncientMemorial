using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UBools {
	[Serializable]
	public class UAnd : UValue<bool> {
		public override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamic || B.isDynamic;
			}
		}
		
		[Header("A && B")]
		[SerializeReference] [SubclassSelector] public UValue<bool> A;
		[SerializeReference] [SubclassSelector] public UValue<bool> B;
		
		public override bool getValue => A.value && B.value;
	}
}