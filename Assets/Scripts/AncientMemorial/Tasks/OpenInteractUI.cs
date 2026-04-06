using AncientMemorial.Interactions;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.String;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UnityEngine;

namespace AncientMemorial.Tasks {
	public class OpenInteractUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas    canvas;
		public float      spawnTime;

		public UString ID;
		public UString Category;
		
		public override void Execute(ITaskable self) {
			GameObject ui  = UUIManager.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();
			((InteractProgressValue)((USliderAction)uui.GetAction("ProgressBar")).value).obj = (Interaction)self;
			((UTextAction)uui.GetAction("TextLabel")).SetText(((Interaction)self).InteractText);
			
			if (ID       !=null) uui.ID        = ID.GetText();
			if (Category !=null) uui.UCategory = Category.GetText();
		}
	}
}