using System;
using AncientMemorial.AMObjects;
using AncientMemorial.ObjectPool;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ReleaseObject : Task {
		public float releaseTime;
		
		public string ID;
		
		public override void Execute(ITaskable self) {
			AMObjectPool.instance.Release(AMObject.GetAMObject(ID).gameObject, releaseTime);
		}
	}
}