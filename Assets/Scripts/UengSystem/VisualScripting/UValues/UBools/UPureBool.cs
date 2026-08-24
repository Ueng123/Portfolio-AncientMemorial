using System;

namespace UengSystem.VisualScripting.UValues.UBools {
	[Serializable]
	public class UPureBool : UPureValue<bool, UPureBool> {
		// protected override bool getIsDynamic => false;
		//
		// [FormerlySerializedAs("boolValue")]public bool oldValue;
		//
		// public override void OnAfterDeserialize() {
		// 	base.OnAfterDeserialize();
		// 	pureValue = oldValue;
		// }
		//
		// protected override bool getValue {
		// 	get {
		// 		pureValue = oldValue;
		// 		return oldValue;
		// 	}
		// }
	}
}