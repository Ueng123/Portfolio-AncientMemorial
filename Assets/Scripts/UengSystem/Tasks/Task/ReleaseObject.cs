using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UnityEngine;

namespace UengSystem.Tasks {
	[Serializable]
	public class ReleaseObject : TaskComponent {
		public float releaseTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		
		public override void Execute(ITaskable self) {
			Debug.Log(ID.value);
			if (ID.value == "this") {
				UObjectPool.instance.Release((self as UObject)?.gameObject, releaseTime);
			}
			else {
				UObjectPool.instance.Release(UObject.GetUObject(ID.getValue).gameObject, releaseTime);
			}
		}
	}
}