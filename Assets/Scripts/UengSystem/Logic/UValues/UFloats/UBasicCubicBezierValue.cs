using System;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UBasicCubicBezierValue : UValue<float> {
		protected override bool getIsDynamic {
			get {
				startSlope.parent  = this;
				endSlope.parent    = this;
				T.parent           = this;

				return true;
			}
		}
		
		[SerializeReference] [SubclassSelector] public UValue<float> startSlope;
		[SerializeReference] [SubclassSelector] public UValue<float> endSlope;
		
		[SerializeReference] [SubclassSelector] public UValue<float> T;

		private CubicBezier getCurve => new CubicBezier(
			0,
			startSlope.value,
			1,
			endSlope.value);
		
		private CubicBezier curveCache = default;
		public CubicBezier curve {
			get {
				if (curveCache.Equals(default)) {
					curveCache = getCurve;
				}

				if (   Mathf.Approximately(startSlope.value,  curveCache.startSlope)
					&& Mathf.Approximately(endSlope.value,    curveCache.endSlope)) {
					return curveCache;
				}
				
				curveCache       = getCurve;
				return curveCache;
			}
		}

		protected override float getValue => curve.GetValue(T.value);
	}
}