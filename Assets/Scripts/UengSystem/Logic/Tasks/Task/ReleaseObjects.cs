using System;
using UengSystem.Logic.UValues;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ReleaseObjects : TaskComponent {
		public float releaseTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"          +
					  $" > Category = {Category}\n" +
					  $"");
			
			foreach (UObject obj in UObject.GetUObjects(Category.value)) {
				UObjectPool.instance.Release(obj.gameObject, releaseTime);
			}
		}
	}
}