using UnityEngine;

namespace UengSystem.Utility {
	public class StopWatch {

		// 인스턴스 프로퍼티
		private float tickTime;
		private bool  ticked;
		
		private float stopTime;
		private bool  paused;

		// 인스턴스 메서드
		public StopWatch Tick() {
			ticked = true;
			tickTime = Time.time;

			return this;
		}

		public StopWatch TryTick() {
			return ticked ? this : Tick();
		}

		public float Tock() {
			if (!ticked) throw new System.Exception("StopWatch not ticked");
			
			return Time.time - tickTime;
		}

		public float TryTock(float baseValue) {
			if (!ticked) return baseValue;
			
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
		
		public bool CheckIn(float margin, bool baseValue = false) {
			return ticked?Tock() < margin:baseValue;
		}

		public bool CheckOut(float margin, bool baseValue = false) {
			return ticked?Tock() >= margin:baseValue;
		}
	}
}
