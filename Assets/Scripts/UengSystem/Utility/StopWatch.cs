using UnityEngine;

namespace UengSystem.Utility {
	public class StopWatch {
		private float tickTime;
		private bool  ticked;
		
		private float stopTime;
		private bool  paused;

		public void Tick() {
			ticked = true;
			tickTime = Time.time;
		}

		public float Tock() {
			if (!ticked) throw new System.Exception("StopWatch not ticked");
			
			return Time.time - tickTime;
		}

		public void Pause() {
			if (!ticked) return;
			if (paused) return;
			paused = true;
			
			stopTime = Time.time;
		}

		public float Resume() {
			if (!paused) return 0;
			paused = false;
			
			float totalStoppedTime = Time.time - stopTime;
			tickTime += totalStoppedTime;
			return totalStoppedTime;
		}

		public bool Check(float margin, float time = 0) {
			return ticked && Mathf.Abs(Tock() - time) < margin;
		}
	}
}