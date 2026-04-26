using System;
using UengSystem.Logic.UValues;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ConditionalTask : TaskComponent {
		[FormerlySerializedAs("condition")] [SerializeReference] [SubclassSelector]
		public UValue<bool> boolean;
		public Task task;
		
		[HideInInspector]
		public bool isTrue;
		public bool detectOnce;
		
		public override void Execute(ITaskable self) {
			bool result = boolean.getValue;
			if (result) {
				if (detectOnce && isTrue) return;
				task.Execute(self);
			} 
			isTrue = result;
		}
	}
}