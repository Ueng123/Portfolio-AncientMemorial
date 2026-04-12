using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.String;

namespace UengSystem.Tasks {
	[Serializable]
	public class ReleaseObject : TaskComponent {
		public float releaseTime;
		
		public UString ID;
		
		public override void Execute(ITaskable self) {
			if (ID.GetText() == "this") {
				UObjectPool.instance.Release((self as UObject)?.gameObject, releaseTime);
			}
			else {
				UObjectPool.instance.Release(UObject.GetUObject(ID.GetText()).gameObject, releaseTime);
			}
		}
	}
}