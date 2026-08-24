using System;
using UengSystem.UI.UUIs;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class AddInfoMessage : TaskComponent {
		
		[SerializeReference][SubclassSelector]
		public UValue<string> message;
		
		public override void Execute(ITaskable self) {
			InfoUUI.instance.AddInfoMessage(message.value);
		}
	}
}