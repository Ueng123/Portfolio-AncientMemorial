using System;
using Color = UnityEngine.Color;

namespace UengSystem.Logic.UValues.UColors {
	[Serializable]
	public class USimpleColor : UValue<Color> {
		protected override bool getIsDynamic => false;
		
		public             Color color;
		protected override Color getValue => color;
	}
}