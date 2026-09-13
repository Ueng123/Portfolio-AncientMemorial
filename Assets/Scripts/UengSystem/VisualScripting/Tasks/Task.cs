using System;
using System.Collections;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UengSystem.UDebug;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class Task {
		[SerializeField] private bool useRealTime;
		[SerializeField] private TaskListItem[] tasks;

		private UObject coroutineRunner;
		private Coroutine runningTask;
		private float elapsed;

		public void Execute(ITaskable self, UObject coroutineRunner = null) {
			this.coroutineRunner = coroutineRunner ?? GlobalObject.instance;
			elapsed = 0f;
			runningTask = this.coroutineRunner.StartCoroutine(ExecuteEnumerator(self));
		}

		public IEnumerator ExecuteEnumerator(ITaskable self) {
			if (tasks == null) yield break;

			elapsed = 0f;
			UObject owner = self as UObject;
			bool hasOwner = owner && owner.lifeCycle.isPrepared;
			long life = owner ? owner.lifeNumber : 0;
			bool releasing = owner && owner.lifeCycle.phase == LifeCyclePhase.Releasing;

			foreach (TaskListItem taskItem in tasks) {
				if (!IsValidOwner(owner, hasOwner, life, releasing)) yield break;

				float wait = taskItem.time - elapsed;
				elapsed = taskItem.time;
				if (wait > 0) {
					bool isRealTime = useRealTime || self is UI.UUI;
					yield return isRealTime ? new WaitForSecondsRealtime(wait) : CacheManager.WaitForSecondsCeiling(wait);
				}

				foreach (TaskComponent task in taskItem.tasks) {
					if (!IsValidOwner(owner, hasOwner, life, releasing)) yield break;
					try { task?.Execute(self); }
					catch (Exception error) {
						DebugManager.LogError($"Task > {task?.GetType().Name}.Execute({self.GetType().Name})", error,
							owner ? owner.gameObject : null);
					}
				}
			}
		}


		public void CancelTasks() {
			if (runningTask == null) return;
			foreach (TaskListItem taskItem in tasks) {
				foreach (TaskComponent task in taskItem.tasks) {
					task.StopAllCoroutines();
				}
			}
			
			coroutineRunner?.StopCoroutine(runningTask);
			runningTask = null;
		}
		
		private static bool IsValidOwner(UObject owner, bool hasOwner, long life, bool releasing) {
			if (UObject.isStoppingLifeCycles) return false;
			if (!hasOwner) return true;
			return owner && owner.lifeNumber == life && !owner.isReleased && !owner.lifeCycle.isShuttingDown
			       && (releasing || owner.lifeCycle.phase != LifeCyclePhase.Releasing);
		}
	}
}
