using System;
using Color = UnityEngine.Color;

namespace UengSystem.Logic.UValues.UColors {
	[Serializable]
	public class USimpleColor : UValue<Color> {
		public override bool getIsDynamic => false;
		
		public Color color;
		public override Color getValue => color;
	}
}