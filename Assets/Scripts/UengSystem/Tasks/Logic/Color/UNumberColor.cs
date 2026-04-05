using System;
using UengSystem.Tasks.Logic.Value;
using UnityEngine;

namespace UengSystem.Tasks.Logic.Color {
	[Serializable]
	public class UNumberColor : UColor {
		[SerializeReference] [SubclassSelector]
		public UNumber r;
		
		[SerializeReference] [SubclassSelector]
		public UNumber g;
		
		[SerializeReference] [SubclassSelector]
		public UNumber b;

		public override UnityEngine.Color GetColor() {
			return new UnityEngine.Color(r.GetValue(), g.GetValue(), b.GetValue());
		}
	}
}