using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class SetCrystalMessage : TaskComponent {

		[SerializeReference] [SubclassSelector]
		public UValue<string> newMessage;
		
		public override void Execute(ITaskable self) {
			GameManager.instance.Crystal.interactionText = newMessage;
		}
	}
}