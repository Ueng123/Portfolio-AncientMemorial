using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public abstract class TaskComponent : InspectorItem {
		private readonly List<TaskExecution> Executions = new();
		public Coroutine StartCoroutine(IEnumerator enumerator) {
			Executions.RemoveAll(Execution => !Execution.isRunning);
			TaskExecution Execution = TaskExecution.Current?.StartChild(enumerator)
			                          ?? new TaskExecution(enumerator, null).Start();
			Executions.Add(Execution);
			return Execution.coroutine;
		}

		public void StopCoroutine(Coroutine coroutine) {
			if (coroutine == null) return;
			foreach (TaskExecution Execution in Executions.ToArray()) {
				if (Execution.coroutine == coroutine) Execution.Cancel();
			}
		}

		public void StopAllCoroutines() {
			foreach (TaskExecution Execution in Executions) Execution.Cancel();
			Executions.Clear();
		}
		
		public abstract void Execute(ITaskable self);
	}
}
