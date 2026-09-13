using System;
using System.Collections;
using UengSystem.Managers;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UAction {
	public class DelayedAction : UAction {

		// 정적 프로퍼티
		public static int runningDelayedActionCount = 0;

		// 인스턴스 프로퍼티
		private float  startTime;
		public  float  delay;
		private Action actionToDelay;
		private Action actionOnCancel;
		private bool   delayInRealTime = false;
		
		private Coroutine process;

		private IActionable executor;

		private WaitForSeconds waitForSeconds;
		private WaitForSecondsRealtime waitForSecondsRealtime;

		// 인스턴스 메서드
		public DelayedAction(float delay, Action actionToDelay, Action actionOnCancel = null, IActionable executor = null) {
			this.executor       = executor;
			this.delay          = delay;
			this.actionToDelay  = actionToDelay;
			this.actionOnCancel = actionOnCancel;
			
			// 예약된 지연 시간보다 일찍 실행되지 않도록 올림 캐시 사용
			waitForSeconds  = CacheManager.WaitForSecondsCeiling(delay);
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

		// 0~1
		public float GetProgress() {
			if (!Executing) return 0;
			return (Time.time - startTime) / delay;
		}

		// 오버라이드 메서드
		public override void Done() { DoneDA(); }
		
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
		
		protected override IEnumerator ActionEnumerator() {
			yield return delayInRealTime? waitForSecondsRealtime : waitForSeconds;
			DoneDA(false);
		}
	}
}
