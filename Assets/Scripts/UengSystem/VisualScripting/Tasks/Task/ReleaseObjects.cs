using System;
using System.Collections.Generic;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UDebug;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ReleaseObjects : TaskComponent {
		public bool PlayEffect = true;
		
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"          +
							 $" > Category = {Category}\n" +
							 $"");

			List<UObject> targetObjects = UObject.GetUObjects(Category.value);

			for (int i = targetObjects.Count - 1; i >= 0; i--) {
				UObject obj = targetObjects[i];
				obj.Release(PlayEffect);
			}
		}
	}
}
