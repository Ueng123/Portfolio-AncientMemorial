using System;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ConditionalTask : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<bool> boolean;
		public Task taskOnTrue;
		public Task taskOnFalse;

		public bool? boolValueCache;
		public ConditionalTaskType type;
		
		public override void Execute(ITaskable self) {
			boolValueCache ??= boolean.value;
			bool result = boolean.value;
			
			switch (type) {
				case ConditionalTaskType.ExecuteEveryFrame:
					if (result) taskOnTrue?.Execute(self);
					else taskOnFalse?.Execute(self);
					return;
				case ConditionalTaskType.ExecuteStateChanged:
					if (result == boolValueCache) return;
					if (result) taskOnTrue?.Execute(self);
					else taskOnFalse?.Execute(self);
					boolValueCache = result;
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}