using System;
using AncientMemorial.Interactions;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UColors;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class OpenInteractUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas    canvas;

		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			GameObject ui  = UUIManager.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();
			
			uui.GetAction<UTextAction>("TextLabel").SetText(((Interaction)self).InteractText);
			
			if (ID       !=null) uui.ID        = ID.getValue;
			if (Category !=null) uui.UCategory = Category.getValue;
		}
	}
}