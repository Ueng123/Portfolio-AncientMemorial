using System;
using System.Collections;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class RepeatTask : TaskComponent {
		public bool            delayFirst;
		public bool            waitForTask;
		
		[SerializeReference] [SubclassSelector]
		public UValue<UObject> taskRunner;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> delay;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> count;

		public Task task;
		
		public override void Execute(ITaskable self) {
			MonoBehaviour coroutineRunner = (taskRunner!=null) ? taskRunner.value : CoroutineRunner.instance;
			
			StartCoroutine(Run(self));
		}

		public IEnumerator Run(ITaskable self) {
			int countInt = (int)count.value;
			for (int i = 0; i < countInt; i++) {
				if (delayFirst) yield return new WaitForSeconds(delay.value);

				if (waitForTask) yield return task.ExecuteEnumerator(self);
				else task.Execute(self);
				
				if (!delayFirst) yield return new WaitForSeconds(delay.value);
			}
		}
	}
}