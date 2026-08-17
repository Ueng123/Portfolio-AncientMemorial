using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	public class UCurrTime : UValue<float> {
		protected override bool  getIsDynamic => true;
		protected override float getValue     => Time.time;
	}
}