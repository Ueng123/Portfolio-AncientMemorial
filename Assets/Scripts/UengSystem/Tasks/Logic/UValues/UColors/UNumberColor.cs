using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues.UColors {
	[Serializable]
	public class UNumberColor : UValue<Color> {
		public override bool getIsDynamic => true;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> r;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> g;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> b;

		public override Color getValue => new (r.getValue, g.getValue, b.getValue);
	}
}