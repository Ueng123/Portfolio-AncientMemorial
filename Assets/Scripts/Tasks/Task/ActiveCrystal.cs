using System;
using AncientMemorial.Interactions;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ActiveCrystal : Task {
		public bool   changeLabel;
		public string newLabelText;
		
		public override void Execute(ITaskable self) {
			Crystal crystal = GameManager.instance.Crystal;

			crystal.interactable = true;
			if (changeLabel) { crystal.ChangeInteractText(newLabelText); }
		}
	}
}