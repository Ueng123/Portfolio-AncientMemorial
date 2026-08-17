using System;

namespace UengSystem.Utility {
	public struct CubicBezier : IEquatable<CubicBezier> {
		public readonly float startValue;
		public readonly float endValue;
		public readonly float startSlope;
		public readonly float endSlope;
		
		public float GetValue(float nt) {
			float lt = 1 - nt;
			
			float B1 = 1 * lt * lt * lt * startValue;
			float B2 = 3 * nt * lt * lt * startSlope;
			float B3 = 3 * nt * nt * lt * endSlope;
			float B4 = 1 * nt * nt * nt * endValue;
			float B  = B1 + B2 + B3 + B4;
			
			return B;
		}
		
		public CubicBezier(float startValue, float startSlope, float endValue, float endSlope) {
			this.startValue = startValue;
			this.startSlope = startSlope;
			this.endValue   = endValue;
			this.endSlope   = endSlope;
		}

		public bool Equals(CubicBezier other) {
			return startValue.Equals(other.startValue)
				   && endValue.Equals(other.endValue)
				   && startSlope.Equals(other.startSlope)
				   && endSlope.Equals(other.endSlope);
		}
	}
}