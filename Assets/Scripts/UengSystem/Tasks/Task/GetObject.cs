using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UnityEngine;

namespace UengSystem.Tasks {
	[Serializable]
	public class GetObject : TaskComponent {
		public GameObject TargetObject;
		public Vector2    position;
		public float      spawnTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			GameObject obj = UObjectPool.instance.Get(TargetObject.name, position, spawnTime);
			UObject amobj = obj.GetComponent<UObject>();

			if (ID       !=null) amobj.ID       = ID.getValue;
			if (Category !=null) amobj.Category = Category.getValue;
		}
	}
}