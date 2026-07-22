using System;
using System.Collections;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Utility {
	public class DelayedAction : UAction {

		private float   startTime;
		public  float   delay;
		private Action  actionToDelay;
		private Action  actionOnCancel;
		private bool    isUObjectDelayed;
		
		private Coroutine process;

		private IActionable executor;

		public DelayedAction(float delay, Action actionToDelay, Action actionOnCancel = null, IActionable executor = null) {
			this.executor       = executor;
			this.delay          = delay;
			this.actionToDelay  = actionToDelay;
			this.actionOnCancel = actionOnCancel;
		}

		public DelayedAction ExecuteDA(bool delayInRealTime = false) {
			if (Executing) return this;
			
			if (delay == 0) {
				actionToDelay();
				return this;
			}
			
			startTime = Time.time;
			Executing = true;
			
			Execute(executor);
			this.delayInRealTime = delayInRealTime;
			process = GlobalCoroutineRunner.instance.StartCoroutine(ActionEnumerator());
			return this;
		}

		public override void Done() { DoneDA(); }

		public void DoneDA(bool stopCoroutine = true) {
			if (!Executing) return;
			
			if (stopCoroutine) GlobalCoroutineRunner.instance.StopCoroutine(process);
			actionToDelay.Invoke();
			Executing = false;
		}
		
		public override void Cancel() {
			if (!Executing) return;

			GlobalCoroutineRunner.instance.StopCoroutine(process);
			actionOnCancel?.Invoke();
			Executing = false;
		}

		// 0~1
		public float GetProgress() {
			if (!Executing) return 0;
			return (Time.time - startTime) / delay;
		}

		bool delayInRealTime = false;
		protected override IEnumerator ActionEnumerator() {
			yield return (delayInRealTime)? new WaitForSecondsRealtime(delay) : new WaitForSeconds(delay);
			DoneDA(false);
		}
	}
}