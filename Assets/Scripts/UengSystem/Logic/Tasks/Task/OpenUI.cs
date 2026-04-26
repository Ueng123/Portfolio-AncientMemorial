using System;
using UengSystem.Logic.UValues;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class OpenUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas    canvas;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			GameObject ui  = UUIManager.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();

			if (ID.value       != "") uui.ID        = ID.getValue;
			if (Category.value != "") uui.UCategory = Category.getValue;
		}
	}
}