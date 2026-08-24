using System;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UPureString : UPureValue<string, UPureString> {
		// protected override bool getIsDynamic => false;
		//
		// public override void OnAfterDeserialize() {
		// 	base.OnAfterDeserialize();
		// 	pureValue = oldValue;
		// }
		//
		// [FormerlySerializedAs("oldValue")] [FormerlySerializedAs("Text")] public string pureValue;
		// protected override                                                       string getValue => pureValue;
	}
}