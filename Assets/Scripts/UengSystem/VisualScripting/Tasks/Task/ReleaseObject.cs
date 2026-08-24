using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UDebug;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ReleaseObject : TaskComponent {
		public float releaseTime;

		public bool releaseThis;
		
		[SerializeReference][SubclassSelector] public UValue<UObject> TargetObject;
		
		public override void Execute(ITaskable self) {
			UObject target = releaseThis?(UObject)self:TargetObject.value;
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"                     +
							 $" > releaseThis = {releaseThis}\n"      +
							 $" > TargetObject = {target.name}\n"     +
							 $" > --- TargetUUI DATA ---\n"           +
							 $" > --- ID = {target.ID}\n"             +
							 $" > --- Category = {target.Category}\n" +
							 $"");
		
			UObjectPool.instance.Release(target.gameObject, releaseTime);
		}
	}
}