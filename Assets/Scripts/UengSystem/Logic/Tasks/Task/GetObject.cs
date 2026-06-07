using System;
using UengSystem.Logic.UValues;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class GetObject : TaskComponent {
		public                                        GameObject      TargetObject;
		[SerializeReference][SubclassSelector] public UValue<Vector2> position;
		[SerializeReference][SubclassSelector] public UValue<float>   spawnTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) { 
			// Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			// Debug.Log($" >>> Given Data\n"                       +
			// 		  $" > ID = {ID.value}\n"                    + 
			// 		  $" > Category = {Category.value}\n"        + 
			// 		  $" > targetPrefab = {TargetObject.name}\n" + 
			// 		  $" > position = {position.value}\n"              + 
			// 		  $" > duration = {spawnTime.value}\n"           + 
			// 		  $"");
			
			GameObject obj  = UObjectPool.instance.Get(TargetObject.name, position.value, spawnTime.value);
			UObject    uobj = obj.GetComponent<UObject>();

			if (ID       !=null) uobj.ID       = ID.getValue;
			if (Category !=null) uobj.Category = Category.getValue;
		}
	}
}