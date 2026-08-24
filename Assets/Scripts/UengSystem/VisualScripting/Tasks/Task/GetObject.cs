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
		[SerializeReference][SubclassSelector] public UValue<float>   spawnTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"                                                +
							 (ID       != null ? $" > ID = {ID.value}\n" : "")                   +
							 (Category != null ? $" > Category = {Category.value}\n" : "")       +
							 $" > targetPrefab = {TargetObject.name}\n"                          +
							 $" > position = {position.value}\n"                                 +
							 $" > duration = {spawnTime?.value??0}\n" +
							 $"");
			
			GameObject obj  = UObjectPool.instance.Get(TargetObject.name, position.value, spawnTime?.value??0);
			UObject    uObj = obj.GetComponent<UObject>();

			if (!string.IsNullOrWhiteSpace(ID?.value)) uObj.ID             = ID.value;
			if (!string.IsNullOrWhiteSpace(Category?.value)) uObj.Category = Category.value;
		}
	}
}