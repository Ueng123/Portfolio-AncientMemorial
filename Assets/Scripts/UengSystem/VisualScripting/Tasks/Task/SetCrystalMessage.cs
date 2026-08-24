using System;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SetCrystalMessage : TaskComponent {

		[SerializeReference] [SubclassSelector]
		public UValue<string> newMessage;
		
		public override void Execute(ITaskable self) {
			GameManager.instance.Crystal.interactionText = newMessage;
		}
	}
}