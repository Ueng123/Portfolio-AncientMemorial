using System;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SetUColorVariable : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<Color> value;
		
		public override void Execute(ITaskable self) {
			GameManager.UValueColorVariables[key.value] = value;
		}
	}
}