using System;
using UengSystem.UI.UInputFields;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UInputFieldValue : UValue<string> {

		// 인스턴스 프로퍼티
		protected override bool   getIsDynamic => true;
		protected override string getValue     => UInputFieldAction.inputFieldTextVariable.value;
	}
}