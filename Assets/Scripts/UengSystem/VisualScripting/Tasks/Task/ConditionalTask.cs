using System;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ConditionalTask : TaskComponent {

		// 인스턴스 프로퍼티
		[SerializeReference] [SubclassSelector]
		public UValue<bool> boolean;
		public Task taskOnTrue;
		public Task taskOnFalse;

		public bool? boolValueCache;
		public ConditionalTaskType type;

		// 오버라이드 메서드
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