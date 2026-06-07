using UnityEngine;

namespace UengSystem.Utility {
	public class StopWatch {
		private float tickTime;

		public void Tick() {
			tickTime = Time.time;
		}

		public float Tock() {
			return Time.time - tickTime;
		}

		public bool Check(float margin, float time = 0) {
			return Mathf.Abs(Tock() - time) < margin;
		}
	}
}