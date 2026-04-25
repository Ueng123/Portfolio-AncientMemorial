using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Tasks.Logic.UValues.UStrings;
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

			if (ID.getValue       != "") uui.ID        = ID.getValue;
			if (Category.getValue != "") uui.UCategory = Category.getValue;
		}
	}
}