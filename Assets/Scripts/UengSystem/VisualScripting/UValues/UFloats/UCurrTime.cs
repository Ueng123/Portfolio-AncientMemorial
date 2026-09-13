using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	public class UCurrTime : UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool  getIsDynamic => true;
		protected override float getValue     => Time.time;
	}
}