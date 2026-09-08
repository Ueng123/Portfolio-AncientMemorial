using System;
using System.Collections;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.UAction {
	public class DelayedAction : UAction {
		public static int runningDelayedActionCount = 0;
		
		private float  startTime;
		public  float  delay;
		private Action actionToDelay;
		private Action actionOnCancel;
		private bool   delayInRealTime = false;
		
		private Coroutine process;

		private IActionable executor;

		private WaitForSeconds waitForSeconds;
		private WaitForSecondsRealtime waitForSecondsRealtime;
		
		public DelayedAction(float delay, Action actionToDelay, Action actionOnCancel = null, IActionable executor = null) {
			this.executor       = executor;
			this.delay          = delay;
			this.actionToDelay  = actionToDelay;
			this.actionOnCancel = actionOnCancel;
			
			waitForSeconds  = new WaitForSeconds(delay);
			waitForSecondsRealtime = new WaitForSecondsRealtime(delay);
		}

		public DelayedAction ExecuteDA(bool delayInRealTime = false) {
			if (Executing) return this;
			if (executor is Objects.UObject { canStartOwnedWork: false }) return this;
			
			if (delay == 0) {
				actionToDelay();
				return this;
			}
			
			startTime                 =  Time.time;
			
			Executing = true;
			runningActionCount        += 1;
			runningDelayedActionCount += 1;
			
			Execute(executor);
			if (!Executing) return this;
			this.delayInRealTime = delayInRealTime;
			process = GlobalObject.instance.StartCoroutine(ActionEnumerator());
			return this;
		}

		public override void Done() { DoneDA(); }

		public void DoneDA(bool stopCoroutine = true) {
			if (!Executing) return;
			
			if (stopCoroutine && process != null && GlobalObject.instance) GlobalObject.instance.StopCoroutine(process);
			process = null;
			Executing                 =  false;
			runningActionCount        -= 1;
			runningDelayedActionCount -= 1;
			executor?.UnregisterAction(this);
			actionToDelay.Invoke();
		}
		
		public override void Cancel() {
			if (!Executing) return;

			if (process != null && GlobalObject.instance) GlobalObject.instance.StopCoroutine(process);
			process = null;
			Executing                 =  false;
			runningActionCount        -= 1;
			runningDelayedActionCount -= 1;
			executor?.UnregisterAction(this);
			actionOnCancel?.Invoke();
		}

		// 0~1
		public float GetProgress() {
			if (!Executing) return 0;
			return (Time.time - startTime) / delay;
		}
		
		protected override IEnumerator ActionEnumerator() {
			yield return delayInRealTime? waitForSecondsRealtime : waitForSeconds;
			DoneDA(false);
		}
	}
}
