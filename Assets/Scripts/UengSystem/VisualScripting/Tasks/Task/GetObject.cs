using UengSystem.Utility;
using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UDebug;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class GetObject : TaskComponent {

		// 인스턴스 프로퍼티
		private int? TargetPrefabId;
		private GameObject CachedTargetPrefab;
		private int targetPrefabId {
			get {
				if (CachedTargetPrefab != TargetObject) {
					CachedTargetPrefab = TargetObject;
					TargetPrefabId = null;
				}
				TargetPrefabId ??= TargetObject.name.GetHash();
				return TargetPrefabId.Value;
			}
		}

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
			
			UObject.Get(targetPrefabId, position.value, PlayEffect, Obj => {
				if (!string.IsNullOrWhiteSpace(ID?.value)) Obj.ID = ID.value;
				if (!string.IsNullOrWhiteSpace(Category?.value)) Obj.Category = Category.value;
			});
		}
	}
}
