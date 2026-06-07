using System;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class AddUFloatVariable : TaskComponent {
		
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> value;
		
		public override void Execute(ITaskable self) {
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"          +
					  $" > key   = {key.value}\n"   +
					  $" > value = {value.value}\n" +
					  $"");

			GameManager.UValueFloatVariables[key.value] = new UAdd {
				A = GameManager.UValueFloatVariables[key.value],
				B = value 
			};
		}
	}
}