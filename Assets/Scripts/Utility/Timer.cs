using UnityEngine;

namespace AncientMemorial {
	public class Timer {
		private float startTime;
		private float duration;
		
		public Timer(float duration) {
			startTime     = Time.time;
			this.duration = duration;
		}

		public float GetProgress(bool inNormalized) {
			return (inNormalized) ? (startTime - Time.time) / duration : startTime - Time.time;
		}
	}
}