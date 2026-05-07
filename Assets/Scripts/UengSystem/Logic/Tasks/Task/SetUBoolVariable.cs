using System;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SetUBoolVariable : TaskComponent {
			
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<bool> value;
		
		public override void Execute(ITaskable self) {
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"          +
					  $" > key   = {key.value}\n"   +
					  $" > value = {value.value}\n" +
					  $"");
			
			GameManager.UValueBoolVariables[key.value] = value;
		}
	}
}