using System;
using UengSystem.UI.UInputFields;

namespace UengSystem.Logic.UValues.UStrings {
	[Serializable]
	public class UInputFieldValue : UValue<string> {
		public override bool   getIsDynamic => true;
		public override string getValue     => UInputFieldAction.inputFieldTextVariable.value;
	}
}