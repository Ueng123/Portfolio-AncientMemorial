using System;
using System.Collections;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class Task {

		[SerializeField] private bool           useRealTime;
		[SerializeField] private TaskListItem[] tasks;
		
		private UObject coroutineRunner;
		private Coroutine runningTask;
		
		private float waitedTime = 0f;
		
		public void Execute(ITaskable self, UObject coroutineRunner = null) {
			this.coroutineRunner = coroutineRunner??CoroutineRunner.instance;
			waitedTime           = 0f;
			runningTask          = this.coroutineRunner.StartCoroutine(ExecuteEnumerator(self));
		}

		public IEnumerator ExecuteEnumerator(ITaskable self) {
			foreach (TaskListItem taskItem in tasks) {
				float wait = taskItem.time - waitedTime;
				waitedTime = taskItem.time;
				if (wait != 0) yield return useRealTime?new WaitForSecondsRealtime(wait):new WaitForSeconds(wait);
				foreach (TaskComponent task in taskItem.tasks) {
					try { task?.Execute(self); }
					catch (Exception _e) {
						DebugManager.LogError($"Task > {task?.GetType().Name}.Execute({self.GetType().Name})", _e, ((UObject)self).gameObject);
					}
				}
			}
		}

		public void CancelTasks() {
			if (runningTask != null) {
				foreach (TaskListItem taskItem in tasks) {
					foreach (TaskComponent task in taskItem.tasks) {
						task.StopAllCoroutines();
					}
				}
				coroutineRunner.StopCoroutine(runningTask);
			}
		}
	}
}