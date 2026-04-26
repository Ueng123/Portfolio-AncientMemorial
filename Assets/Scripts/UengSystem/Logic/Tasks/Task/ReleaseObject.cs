using System;
using UengSystem.Logic.UValues;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ReleaseObject : TaskComponent {
		public float releaseTime;

		public bool releaseThis;
		
		[SerializeReference][SubclassSelector] public UValue<UObject> TargetObject;
		
		public override void Execute(ITaskable self) {
			if (releaseThis) {
				UObjectPool.instance.Release((self as UObject)?.gameObject, releaseTime);
			}
			else {
				UObjectPool.instance.Release(TargetObject.value.gameObject, releaseTime);
			}
		}
	}
}