using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	public class UCurrTime : UValue<float> {
		protected override bool  getIsDynamic => true;
		protected override float getValue     => Time.time;
	}
}