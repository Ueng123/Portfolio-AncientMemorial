using System;
using System.Collections;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class RepeatTask : TaskComponent {
		public bool delayFirst;
		public bool waitForTask;

		[SerializeReference] [SubclassSelector]
		public UValue<float> delay;
		[SerializeReference] [SubclassSelector]
		public UValue<float> count;

		public Task task;

		public IEnumerator Run(ITaskable self) {
			int countInt = (int)count.value;
			for (int index = 0; index < countInt; index++) {
				if (delayFirst) {
					// UUI 연출은 일시정지 중에도 진행되어야 하므로 RealTime 사용.
					yield return self is UI.UUI ? new WaitForSecondsRealtime(delay.value)
						: CacheManager.WaitForSecondsCeiling(delay.value);
				}

				if (waitForTask) yield return task.ExecuteEnumerator(self);
				else task.Execute(self);

				if (!delayFirst) {
					// 반복 간격은 누락되지 않아야 하므로 Ceiling 캐시 사용.
					yield return self is UI.UUI ? new WaitForSecondsRealtime(delay.value)
						: CacheManager.WaitForSecondsCeiling(delay.value);
				}
			}
		}

		public override void Execute(ITaskable self) => StartCoroutine(Run(self));
	}
}
