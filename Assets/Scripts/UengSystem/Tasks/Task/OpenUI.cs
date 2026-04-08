using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.String;
using UengSystem.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Tasks {
	[Serializable]
	public class OpenUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas    canvas;

		public UString ID;
		public UString Category;
		
		public override void Execute(ITaskable self) {
			GameObject ui  = UUIManager.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();

			if (ID.GetText()       != "") uui.ID        = ID.GetText();
			if (Category.GetText() != "") uui.UCategory = Category.GetText();
		}
	}
}