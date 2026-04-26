using System;
using System.Linq;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UMin : UValue<float> {
		public override bool getIsDynamic {
			get {
				bool result = false;
				foreach (UValue<float> v in values) {
					v.parent = this;
					result = result || v.getIsDynamic;
				}
				return result;
			}
		}

		[SerializeReference][SubclassSelector]
		public UValue<float>[] values;

		public override float getValue {
			get {
				return values.Aggregate(values[0].value, (current, v) => Mathf.Min(current, v.value));
			}
		}
	}
}