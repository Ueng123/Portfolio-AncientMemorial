using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UBools {
	[Serializable]
	public class UAnd : UValue<bool> {
		protected override bool getIsDynamic { 
			get {
				A.parent = this;
				B.parent = this;
				return A.isDynamic || B.isDynamic;
			}
		}
		
		[Header("A && B")]
		[SerializeReference] [SubclassSelector] public UValue<bool> A;
		[SerializeReference] [SubclassSelector] public UValue<bool> B;

		protected override bool getValue => A.value && B.value;
	}
}