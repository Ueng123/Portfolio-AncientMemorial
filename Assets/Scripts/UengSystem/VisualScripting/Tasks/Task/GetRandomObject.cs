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
		public bool PlayEffect = true;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			GameObject TargetObject = TargetObjects[Random.Range(0, TargetObjects.Length)];
			UObject.Get(TargetObject.name, position.value, PlayEffect, Obj => {
				if (!string.IsNullOrWhiteSpace(ID?.value)) Obj.ID = ID.value;
				if (!string.IsNullOrWhiteSpace(Category?.value)) Obj.Category = Category.value;
			});
		}
	}
}
