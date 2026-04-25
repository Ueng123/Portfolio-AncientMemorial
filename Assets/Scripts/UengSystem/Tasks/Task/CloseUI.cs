using System;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UengSystem.UI;

namespace UengSystem.Tasks {
	[Serializable]
	public class CloseUI : TaskComponent {
		public UString ID;
		
		public override void Execute(ITaskable self) {
			UUI uui = UUI.GetUUI(ID.getValue);
			UUIManager.instance.Close(uui.gameObject);
		}
	}
}