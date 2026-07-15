using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.UI;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class AddInfoMessage : TaskComponent {
		
		[SerializeReference][SubclassSelector]
		public UValue<string> message;
		
		public override void Execute(ITaskable self) {
			InfoUUI.instance.AddInfoMessage(message.value);
		}
	}
}