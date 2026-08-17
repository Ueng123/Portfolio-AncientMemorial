using System;
using System.Collections.Generic;
using UengSystem.Logic.UValues;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ReleaseObjects : TaskComponent {
		public float releaseTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"          +
							 $" > Category = {Category}\n" +
							 $"");

			List<UObject> targetObjects = UObject.GetUObjects(Category.value);

			for (int i = targetObjects.Count - 1; i >= 0; i--) {
				UObject obj = targetObjects[i];
				UObjectPool.instance.Release(obj.gameObject, releaseTime);
			}
		}
	}
}