using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UDebug;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class GetObject : TaskComponent {
		public                                        GameObject      TargetObject;
		[SerializeReference][SubclassSelector] public UValue<Vector2> position;
		public bool PlayEffect = true;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"                        +
							 $" > ID = {ID?.value}\n"                    +
							 $" > Category = {Category?.value}\n"        +
							 $" > targetPrefab = {TargetObject?.name}\n" +
							 $" > position = {position?.value}\n"        +
							 $" > PlayEffect = {PlayEffect}\n"  +
							 $"");
			
			UObject.Get(TargetObject.name, position.value, PlayEffect, Obj => {
				if (!string.IsNullOrWhiteSpace(ID?.value)) Obj.ID = ID.value;
				if (!string.IsNullOrWhiteSpace(Category?.value)) Obj.Category = Category.value;
			});
		}
	}
}
