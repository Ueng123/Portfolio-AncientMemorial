using System;
using System.Collections;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class Task {

		[SerializeField] private bool           useRealTime;
		[SerializeField] private TaskListItem[] tasks;
		
		private UObject coroutineRunner;
		private Coroutine runningTask;
		
		public void Execute(ITaskable self, UObject coroutineRunner = null) {
			
			this.coroutineRunner = coroutineRunner??GlobalCoroutineRunner.instance;
			runningTask = this.coroutineRunner.StartCoroutine(ExecuteEnumerator(self));
		}

		public IEnumerator ExecuteEnumerator(ITaskable self) {
			float waitedTime = 0f;
			foreach (TaskListItem taskItem in tasks) {
				float wait = taskItem.time - waitedTime;
				waitedTime = taskItem.time;
				if (wait != 0) yield return useRealTime?new WaitForSecondsRealtime(wait):new WaitForSeconds(wait);
				foreach (TaskComponent task in taskItem.tasks) {
					task?.Execute(self);
				}
			}
		}

		public void CancelTasks() {
			coroutineRunner.StopCoroutine(runningTask);
		}
	}
}