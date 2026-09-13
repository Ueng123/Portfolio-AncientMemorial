using System;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UFloatToInt : UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic {
			get {
				floatValue.parent = this;
				return floatValue.isDynamic;
			}
		}
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> floatValue;

		public int saveFloatDigits;
		
		public FloatToIntegerMode floatToIntegerMode;

		protected override float getValue {
			get {
				int a = (int)Mathf.Pow(10, saveFloatDigits);
				return floatToIntegerMode switch {
					FloatToIntegerMode.Floor => Mathf.Floor(floatValue.value *a)/a,
					FloatToIntegerMode.Round => Mathf.Round(floatValue.value *a)/a,
					FloatToIntegerMode.Ceil  => Mathf.Ceil(floatValue.value*a)/a,
					_                        => throw new ArgumentOutOfRangeException()
				};
			}
		}
	}
}