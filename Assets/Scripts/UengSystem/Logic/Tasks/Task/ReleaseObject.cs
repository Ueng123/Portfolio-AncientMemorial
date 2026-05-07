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
			UObject target = releaseThis?(UObject)self:TargetObject.value;
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"                     +
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