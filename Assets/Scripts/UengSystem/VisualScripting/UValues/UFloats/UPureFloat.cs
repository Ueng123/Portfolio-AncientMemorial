using System;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UPureFloat : UPureValue<float, UPureFloat> {

		// 정적 메서드
		// protected override bool getIsDynamic => false;
		//
		// public override void OnAfterDeserialize() {
		// 	base.OnAfterDeserialize();
		// 	pureValue = oldValue;
		// }
		//
		// [FormerlySerializedAs("number")] public float oldValue;
		// protected override                      float getValue => oldValue;
		
		public static void AddValue(int ID, float amount) {
			UPureFloat var = GetPureValue(ID);
			var.pureValue += amount;
			var.Dirty();
		}
		
		public static void MultValue(int ID, float multiplier) {
			UPureFloat var = GetPureValue(ID, 1);
			var.pureValue *= multiplier;
			var.Dirty();
		}
	}
}