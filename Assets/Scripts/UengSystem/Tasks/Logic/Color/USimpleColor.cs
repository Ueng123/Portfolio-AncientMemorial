using System;

namespace UengSystem.Tasks.Logic.Color {
	[Serializable]
	public class USimpleColor : UColor {
		public UnityEngine.Color color;
		
		public override UnityEngine.Color GetColor() {
			return color;
		}
	}
}