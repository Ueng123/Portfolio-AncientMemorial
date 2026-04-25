using System;
using AncientMemorial.Interactions;
using UengSystem.Managers;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ActiveCrystal : TaskComponent {
		public bool           changeLabel;
		
		[SerializeReference][SubclassSelector]
		public UValue<string> newLabelText;
		
		public override void Execute(ITaskable self) {
			Crystal crystal = GameManager.instance.Crystal;

			crystal.interactable = true;
			if (changeLabel) { crystal.ChangeInteractText(newLabelText); }
		}
	}
}