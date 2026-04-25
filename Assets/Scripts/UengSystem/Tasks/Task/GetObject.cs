using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UnityEngine;

namespace UengSystem.Tasks {
	[Serializable]
	public class GetObject : TaskComponent {
		public GameObject TargetObject;
		public Vector2    position;
		public float      spawnTime;

		public UString ID;
		public UString Category;
		
		public override void Execute(ITaskable self) {
			GameObject obj = UObjectPool.instance.Get(TargetObject.name, position, spawnTime);
			UObject amobj = obj.GetComponent<UObject>();

			if (ID       !=null) amobj.ID       = ID.getValue;
			if (Category !=null) amobj.Category = Category.getValue;
		}
	}
}