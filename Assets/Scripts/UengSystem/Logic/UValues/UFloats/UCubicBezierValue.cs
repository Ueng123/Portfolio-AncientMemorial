using System;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.Logic.UValues.UNumbers {
	[Serializable]
	public class UCubicBezierValue : UValue<float> {
		public override bool getIsDynamic {
			get {
				startValue.parent  = this;
				endValue.parent    = this;
				startSlope.parent  = this;
				endSlope.parent    = this;
				T.parent           = this;

				return true;
			}
		}

		[SerializeReference] [SubclassSelector] public UValue<float> startValue;
		[SerializeReference] [SubclassSelector] public UValue<float> startSlope;
		[SerializeReference] [SubclassSelector] public UValue<float> endValue;
		[SerializeReference] [SubclassSelector] public UValue<float> endSlope;
		
		[SerializeReference] [SubclassSelector] public UValue<float> T;

		private CubicBezier getCurve => new CubicBezier(
			startValue.value,
			startSlope.value,
			endValue.value,
			endSlope.value);
		
		private CubicBezier curveCache = default;
		public CubicBezier curve {
			get {
				if (curveCache.Equals(default)) {
					curveCache = getCurve;
				}

				if (   Mathf.Approximately(startValue.value,  curveCache.startValue)
					&& Mathf.Approximately(startSlope.value,  curveCache.startSlope)
					&& Mathf.Approximately(endValue.value,    curveCache.endValue)
					&& Mathf.Approximately(endSlope.value,    curveCache.endSlope)) {
					return curveCache;
				}
				
				curveCache       = getCurve;
				return curveCache;
			}
		}

		public override float getValue => curve.GetValue(T.value);
	}
}