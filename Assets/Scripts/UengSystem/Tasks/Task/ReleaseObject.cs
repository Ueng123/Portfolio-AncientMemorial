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
			UObjectPool.instance.Release(UObject.GetAMObject(ID.GetText()).gameObject, releaseTime);
		}
	}
}