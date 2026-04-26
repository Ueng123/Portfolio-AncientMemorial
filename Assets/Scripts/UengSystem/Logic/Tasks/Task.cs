using System;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class Task {
		[SerializeField]
		private TaskListItem[]  tasks;
		private DelayedAction[] delayedActions;
		private Coroutine       runningTask;
		
		public void Execute(ITaskable self) {
			delayedActions = new DelayedAction[tasks.Length];

			int i = 0;
			foreach (TaskListItem task in tasks) {
				DelayedAction action = new DelayedAction(
					task.time,
					() => {
						foreach (TaskComponent t in task.tasks) {
							t.Execute(self);
						}
					}
				);

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