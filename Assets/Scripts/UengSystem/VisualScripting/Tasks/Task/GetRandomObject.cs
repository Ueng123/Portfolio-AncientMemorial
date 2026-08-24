using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.VisualScripting.UValues;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class GetRandomObject : TaskComponent {
		[FormerlySerializedAs("prefabs")]      public GameObject[]    TargetObjects;
		[SerializeReference][SubclassSelector] public UValue<Vector2> position;
		[SerializeReference][SubclassSelector] public UValue<float>   spawnTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			GameObject TargetObject = TargetObjects[Random.Range(0, TargetObjects.Length)];
			GameObject obj  = UObjectPool.instance.Get(TargetObject.name, position.value, spawnTime.value);
			UObject    uobj = obj.GetComponent<UObject>();

			if (!string.IsNullOrWhiteSpace(ID?.value)) uobj.ID             = ID.value;
			if (!string.IsNullOrWhiteSpace(Category?.value)) uobj.Category = Category.value;
		}
	}
}