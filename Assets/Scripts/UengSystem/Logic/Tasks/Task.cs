using System;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class Task {
		[SerializeField]
		private TaskListItem[]  tasks;
		private DelayedAction[] delayedActions;
		private Coroutine       runningTask;
		
		public void Execute(ITaskable self, UObject coroutineRunner = null) {
			if (tasks.Length == 0) return;
			
			delayedActions  =   new DelayedAction[tasks.Length];
			coroutineRunner ??= GlobalCoroutineRunner.instance;
			
			int i = 0;
			foreach (TaskListItem task in tasks) {
				DelayedAction action = new (
					task.time,
					() => {
						foreach (TaskComponent t in task.tasks) {
							t.Execute(self);
						}
					},
					coroutineRunner:coroutineRunner);

				action.Execute();
				
				delayedActions[i++] = action;
			}
		}

		public void CancelTasks() {
			foreach (DelayedAction d in delayedActions) {
				d.Cancel();
			}
		}
	}
}