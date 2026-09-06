using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class Task {

		[SerializeField] private bool           useRealTime;
		[SerializeField] private TaskListItem[] tasks;
		
		private readonly List<TaskExecution> Executions = new();
		
		public void Execute(ITaskable self, UObject coroutineRunner = null) {
			Executions.RemoveAll(Execution => !Execution.isRunning);
			Executions.Add(new TaskExecution(ExecuteEnumerator(self), self as UObject, coroutineRunner).Start());
		}

		public IEnumerator ExecuteEnumerator(ITaskable self) {
			float Elapsed = 0;
			if (tasks == null) yield break;
			UObject Owner = self as UObject;
			bool HasOwner = Owner && Owner.lifeCycle.isPrepared;
			long Life = Owner ? Owner.lifeNumber : 0;
			bool Releasing = Owner && Owner.lifeCycle.phase == Objects.LifeCycle.LifeCyclePhase.Releasing;
			foreach (TaskListItem Item in tasks) {
				float Wait = Item.time - Elapsed;
				Elapsed = Item.time;
				if (Wait > 0) yield return useRealTime || self is UI.UUI ? new WaitForSecondsRealtime(Wait) : new WaitForSeconds(Wait);
				foreach (TaskComponent Component in Item.tasks) {
					if (HasOwner && (!Owner || Owner.lifeNumber != Life || Owner.isReleased || Owner.lifeCycle.isShuttingDown
					    || (!Releasing && Owner.lifeCycle.phase == Objects.LifeCycle.LifeCyclePhase.Releasing))) yield break;
					try { Component?.Execute(self); }
					catch (Exception Error) {
						DebugManager.LogError($"Task > {Component?.GetType().Name}.Execute({self.GetType().Name})", Error, Owner ? Owner.gameObject : null);
					}
				}
			}
		}

		public void CancelTasks() {
			foreach (TaskExecution Execution in Executions) Execution.Cancel();
			Executions.Clear();
		}
	}
}
