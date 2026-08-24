using System;
using UengSystem.VisualScripting.UVariables;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues {
	[Serializable]
	public abstract class UPureValue<T, SELF> : UValue<T>, IUPureValue where SELF : UPureValue<T, SELF>, new() {
		protected override bool getIsDynamic => false;
		protected override T    getValue     => pureValue;

		public T pureValue;

		public override UValue<T> OnSet() {
			return new SELF {pureValue = pureValue};
		}

		public static SELF GetPureValue(int ID, T defaultValue = default) {
			if (!UVariable<T>.Get(ID, out UValue<T> value)) {
				value = new SELF {pureValue = defaultValue};
				UVariable<T>.Set(ID, value);
			}

			if (value is not SELF var) {
				throw new InvalidCastException($"{ID} is not a UPureFloat ({ID} is {value.GetType().Name})");
			}
			
			return var;
		}

		public static T GetValue(int ID, T defaultValue = default) {
			return GetPureValue(ID, defaultValue).value;
		}

		public static void SetValue(int ID, T value) {
			SELF var = GetPureValue(ID, value);
			var.pureValue = value;
			var.Dirty();
		}
	}
}