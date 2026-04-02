using System;
using AncientMemorial.Interactions;
using UengSystem.Managers;
using UengSystem.Tasks;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ActiveCrystal : TaskComponent {
		public bool   changeLabel;
		public string newLabelText;
		
		public override void Execute(ITaskable self) {
			Crystal crystal = GameManager.instance.Crystal;

			crystal.interactable = true;
			if (changeLabel) { crystal.ChangeInteractText(newLabelText); }
		}
	}
}