using System;
using UengSystem.UDebug;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.UVariables {
	[Serializable]
	public abstract class UValueFromVariable<T> : UValue<T> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic => true;

		public  string variableName;

		private int? VariableId;
		private int variableID {
			get {
				VariableId ??= variableName.GetHash();
				return VariableId.Value;
			}
		}

		protected override T getValue => UVariable<T>.GetValue(variableID);

		// 오버라이드 메서드
		public override void Dirty() {
			base.Dirty();
			VariableId = null;
		}

	}
}
