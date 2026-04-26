using System;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SetUUIVariable : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<string> key;
		
		[SerializeReference] [SubclassSelector]
		public UValue<UUI> value;
		
		public override void Execute(ITaskable self) {
			GameManager.UValueUUIVariables[key.value] = value;
		}
	}
}