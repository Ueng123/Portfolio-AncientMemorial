using System;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SetUFloatVariable : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> value;
		
		public override void Execute(ITaskable self) {
			GameManager.UValueFloatVariables[key.value] = value;
		}
	}
}