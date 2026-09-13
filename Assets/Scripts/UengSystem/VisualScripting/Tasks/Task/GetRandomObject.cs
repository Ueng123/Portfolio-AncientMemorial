using UengSystem.Utility;
using System;
using UengSystem.Objects;
using UengSystem.VisualScripting.UValues;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class GetRandomObject : TaskComponent {

		// 인스턴스 프로퍼티
		private int[] _targetPrefabIds;
		private int[] targetPrefabIds {
			get {
				if (_targetPrefabIds != null) return _targetPrefabIds;
				
				int[] PrefabIds = new int[TargetObjects.Length];
				for (int Index = 0; Index < TargetObjects.Length; Index++) {
					PrefabIds[Index] = TargetObjects[Index] ? TargetObjects[Index].name.GetHash() : 0;
				}
				
				_targetPrefabIds = PrefabIds;
				return _targetPrefabIds;
			}
		}

		[FormerlySerializedAs("prefabs")]      public GameObject[]    TargetObjects;
		[SerializeReference][SubclassSelector] public UValue<Vector2> position;
		public bool PlayEffect = true;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;

		public override void OnBeforeSerialize() {
			base.OnBeforeSerialize();
		}
		public override void OnAfterDeserialize() {
			base.OnAfterDeserialize();
			_targetPrefabIds = null;
		}

		public override void Execute(ITaskable self) {
			int[] PrefabIds = targetPrefabIds;
			int PrefabId = PrefabIds[Random.Range(0, PrefabIds.Length)];
			UObject.Get(PrefabId, position.value, PlayEffect, Obj => {
				if (!string.IsNullOrWhiteSpace(ID?.value)) Obj.ID = ID.value;
				if (!string.IsNullOrWhiteSpace(Category?.value)) Obj.Category = Category.value;
			});
		}
	}
}
