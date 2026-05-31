using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Utility {
	public class WaitAction {
		
		public readonly  Func<bool> checkCondition;
		private readonly Action     actionToDelay;
		private readonly Action     actionOnCancel;
		
		private static Coroutine  executeCoroutine;
		private static BufferedList<WaitAction> waitActions = new ();
		private static Coroutine process;
		
		public bool  Executing;
		public float startTime;
		public float timeOut;
		
		public WaitAction(Func<bool> checkCondition, Action actionToDelay, Action actionOnCancel = null, float timeOut = 0) {
			this.checkCondition = checkCondition;
			this.actionToDelay = actionToDelay;
			this.actionOnCancel = actionOnCancel;
			this.timeOut = timeOut;
		}

		public void Execute() {
			if (Executing) return;
			if (checkCondition.Invoke()) {
				actionToDelay();
				return;
			}
			
			waitActions.Add(this);
			process ??= GlobalCoroutineManager.instance.StartCoroutine(ExecuteCoroutine());

			startTime = Time.time;
			Executing = true;
		}

		public void Cancel() {
			if (!Executing) return;
			
			actionOnCancel?.Invoke();
			waitActions.Remove(this);
			Executing = false;
		}

		private static IEnumerator ExecuteCoroutine() {
			while (true) {
				yield return null;

				float currTime = Time.time;
				
				foreach (WaitAction waitAction in waitActions) {
					if (waitAction.timeOut != 0 && currTime - waitAction.startTime >= waitAction.timeOut) {
						waitAction.Cancel();
						continue;
					}

					if (waitAction.checkCondition()) {
						waitAction.actionToDelay.Invoke();
						waitAction.Executing = false;
						waitActions.Remove(waitAction);
					}
				}
				
				waitActions.Apply();
				if (waitActions.GetList().Count == 0) break;
			}
			process = null;
		}
		
	}
}