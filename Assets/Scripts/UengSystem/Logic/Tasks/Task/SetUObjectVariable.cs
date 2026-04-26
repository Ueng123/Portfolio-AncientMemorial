using System;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SetUObjectVariable : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<UObject> value;
		
		public override void Execute(ITaskable self) {
			GameManager.UValueUObjectVariables[key.value] = value;
		}
	}
}