using System;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Tasks {
	[Serializable]
	public class CloseUI : TaskComponent {
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		
		public override void Execute(ITaskable self) {
			UUI uui = UUI.GetUUI(ID.getValue);
			UUIManager.instance.Close(uui.gameObject);
		}
	}
}