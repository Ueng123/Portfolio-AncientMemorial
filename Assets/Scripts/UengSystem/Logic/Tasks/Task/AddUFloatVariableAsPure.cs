using System;
using AncientMemorial;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class AddUFloatVariableAsPure : TaskComponent {
		
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> value;
		
		public override void Execute(ITaskable self) {
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"          +
							 $" > key   = {key.value}\n"   +
							 $" > value = {value.value}\n" +
							 $"");

			GameManager.UValueFloatVariables[key.value] = new UPureFloat {
				number = GameManager.UValueFloatVariables[key.value].value + value.value
			};
		}
	}
}