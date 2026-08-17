using System;
using System.Linq;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UMax : UValue<float> {
		protected override bool getIsDynamic {
			get {
				bool result = false;
				foreach (UValue<float> v in values) {
					v.parent = this;
					result = result || v.isDynamic;
				}
				return result;
			}
		}

		[SerializeReference][SubclassSelector]
		public UValue<float>[] values;

		protected override float getValue {
			get {
				return values.Aggregate(values[0].value, (current, v) => Mathf.Max(current, v.value));
			}
		}
	}
}