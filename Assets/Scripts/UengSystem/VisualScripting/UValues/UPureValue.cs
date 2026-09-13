using System;
using UengSystem.VisualScripting.UVariables;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues {
	[Serializable]
	public abstract class UPureValue<T, SELF> : UValue<T>, IUPureValue where SELF : UPureValue<T, SELF>, new() {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic => false;
		protected override T    getValue     => pureValue;

		public T pureValue;

		// 정적 메서드
		public static SELF GetPureValue(int ID, T defaultValue = default) {
			if (!UVariable<T>.Get(ID, out UValue<T> value)) {
				value = new SELF {pureValue = defaultValue};
				UVariable<T>.Set(ID, value);
			}

			if (value is SELF var) return var;
			
			throw new InvalidCastException($"{ID} is not a UPureFloat ({ID} is {value.GetType().Name})");
		}

		public static T GetValue(int ID, T defaultValue = default) {
			return GetPureValue(ID, defaultValue).value;
		}

		public static void SetValue(int ID, T value) {
			SELF var = GetPureValue(ID, value);
			var.pureValue = value;
			var.Dirty();
		}

		// 오버라이드 메서드
		public override UValue<T> OnSet() {
			return new SELF {pureValue = pureValue};
		}
	}
}