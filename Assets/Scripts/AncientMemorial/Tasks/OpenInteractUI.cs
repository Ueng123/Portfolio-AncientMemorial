using System;
using AncientMemorial.Interactions;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.String;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class OpenInteractUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas    canvas;

		public UString ID;
		public UString Category;
		
		public override void Execute(ITaskable self) {
			GameObject ui  = UUIManager.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();
			
			((InteractProgressValue)uui.GetAction<USliderAction>("Bar").value).obj = (Interaction)self;
			uui.GetAction<UTextAction>("TextLabel").SetText(((Interaction)self).InteractText);
			
			if (ID       !=null) uui.ID        = ID.GetText();
			if (Category !=null) uui.UCategory = Category.GetText();
		}
	}
}