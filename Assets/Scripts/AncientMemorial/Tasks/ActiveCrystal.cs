using System;
using AncientMemorial.Interactions;
using UengSystem.Managers;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.UValues.UStrings;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ActiveCrystal : TaskComponent {
		public bool   changeLabel;
		public UString newLabelText;
		
		public override void Execute(ITaskable self) {
			Crystal crystal = GameManager.instance.Crystal;

			crystal.interactable = true;
			if (changeLabel) { crystal.ChangeInteractText(newLabelText); }
		}
	}
}