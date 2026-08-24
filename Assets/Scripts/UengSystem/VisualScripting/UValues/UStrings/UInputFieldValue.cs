using System;
using UengSystem.UI.UInputFields;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UInputFieldValue : UValue<string> {
		protected override bool   getIsDynamic => true;
		protected override string getValue     => UInputFieldAction.inputFieldTextVariable.value;
	}
}