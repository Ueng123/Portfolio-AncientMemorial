using UnityEngine;

namespace UengSystem.Utility {
	public class StopWatch {
		private float tickTime;
		private float stopTime;
		private bool  stopped;
		
		public StopWatch(float startTime = 0) {
			tickTime = startTime;
		}

		public void Tick() {
			tickTime = Time.time;
		}

		public float Tock() {
			return Time.time - tickTime;
		}

		public void Stop() {
			if (stopped) return;
			stopped = true;
			
			stopTime = Time.time;
		}

		public float Resume() {
			if (!stopped) return 0;
			stopped = false;
			
			float totalStoppedTime = Time.time - stopTime;
			tickTime += totalStoppedTime;
			return totalStoppedTime;
		}

		public bool Check(float margin, float time = 0) {
			return Mathf.Abs(Tock() - time) < margin;
		}
	}
}