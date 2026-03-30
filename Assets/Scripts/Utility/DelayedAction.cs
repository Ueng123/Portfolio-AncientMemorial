using System;
using System.Collections;
using System.Diagnostics;
using AncientMemorial.Managers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AncientMemorial {
	public class DelayedAction {

		private float         executeStartTime;
		public  float         delay;
		private Action        actionToDelay;
		private Action        actionOnCancel;
		private MonoBehaviour coroutineRunner;

		public bool Executing;

		private Coroutine process;

		public DelayedAction(float delay, Action actionToDelay, Action actionOnCancel = null, MonoBehaviour coroutineRunner = null) {
			this.delay = delay;
			this.actionToDelay = actionToDelay;
			this.actionOnCancel = actionOnCancel;
			this.coroutineRunner = coroutineRunner ?? GlobalCoroutineManager.instance;
		}

		public void Execute() {
			if (Executing) return;

			executeStartTime = Time.time;
			
			process = coroutineRunner.StartCoroutine(ExecuteCoroutine());
			Executing = true;
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

		private IEnumerator ExecuteCoroutine() {
			yield return new WaitForSeconds(delay);
			
			actionToDelay.Invoke();
			Executing = false;
		}
	}
}