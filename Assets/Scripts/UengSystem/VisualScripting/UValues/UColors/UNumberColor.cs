using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UColors {
	[Serializable]
	public class UNumberColor : UValue<Color> {
		protected override bool getIsDynamic { 
			get {
				r.parent = this;
				g.parent = this;
				b.parent = this;
				return r.isDynamic || g.isDynamic || b.isDynamic || a.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> r;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> g;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> b;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> a;

		protected override Color getValue => new (r.value, g.value, b.value, a.value);
	}
}