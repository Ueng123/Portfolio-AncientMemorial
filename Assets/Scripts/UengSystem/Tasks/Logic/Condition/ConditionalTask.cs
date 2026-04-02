using System;
using UnityEngine;

namespace UengSystem.Tasks.Logic {
	[Serializable]
	public class ConditionalTask : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UCondition condition;

		[SerializeReference] [SubclassSelector]
		public Task task;
		
		[HideInInspector]
		public bool isTrue;
		public bool detectOnce;
		
		public override void Execute(ITaskable self) {
			bool result = condition.Check();
			if (result) {
				if (detectOnce && isTrue) return;
				task.Execute(self);
			} 
			isTrue = result;
		}
	}
}