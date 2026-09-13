using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UTrigFunc : UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic {
			get {
				x.parent = this;
				return x.isDynamic;
			}
		}

		public TrigFunction  funcType;
		[SerializeReference] [SubclassSelector]
		public UValue<float> x;

		protected override float getValue => funcType switch {
			TrigFunction.sin => Mathf.Sin(x.value),
			TrigFunction.cos => Mathf.Cos(x.value),
			TrigFunction.tan => Mathf.Tan(x.value),
			TrigFunction.csc => 1f / Mathf.Sin(x.value),
			TrigFunction.sec => 1f / Mathf.Cos(x.value),
			TrigFunction.cot => 1f / Mathf.Tan(x.value),
			_                => throw new ArgumentOutOfRangeException()
		};
	}
}