using System;
using System.Collections;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Utility {
	public class DelayedAction {

		private float         executeStartTime;
		public  float         delay;
		private Action        actionToDelay;
		private Action        actionOnCancel;
		private UObject       coroutineRunner;
		private bool          isUObjectDelayed;
		
		public bool Executing;

		private Coroutine process;

		public DelayedAction(float delay, Action actionToDelay, Action actionOnCancel = null, UObject coroutineRunner = null) {
			this.delay = delay;
			this.actionToDelay = actionToDelay;
			this.actionOnCancel = actionOnCancel;
			this.coroutineRunner = coroutineRunner ?? GlobalCoroutineRunner.instance;
		}

		public DelayedAction Execute(bool delayInRealTime = false) {
			if (Executing) {
				return this;
			}
			if (delay == 0) {
				actionToDelay();
				return this;
			}
			
			executeStartTime = Time.time;
			
			process = coroutineRunner.StartCoroutine(ExecuteCoroutine(delayInRealTime));
			Executing = true;
			
			return this;
		}

		public void Cancel() {
			if (!Executing) return;

			coroutineRunner.StopCoroutine(process);
			actionOnCancel?.Invoke();
			Executing = false;
		}

		// 0~1
		public float GetProgress() {
			if (!Executing) return 0;
			return (Time.time - executeStartTime) / delay;
		}

		private IEnumerator ExecuteCoroutine(bool delayInRealTime = false) {
			yield return (delayInRealTime)? new WaitForSecondsRealtime(delay) : new WaitForSeconds(delay);
			
			actionToDelay.Invoke();
			Executing = false;
		}
	}
}