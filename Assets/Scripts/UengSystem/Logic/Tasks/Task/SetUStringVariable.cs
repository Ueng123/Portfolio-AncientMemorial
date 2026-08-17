using System;
using AncientMemorial;
using UengSystem.Logic.UValues;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SetUStringVariable : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<string> value;
		
		public override void Execute(ITaskable self) {
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"          +
							 $" > key   = {key.value}\n"   +
							 $" > value = {value.value}\n" +
							 $"");

			GameManager.UValueStringVariables[key.value] = value;
		}
	}
}