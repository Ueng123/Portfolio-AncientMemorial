using System;
using UengSystem.Logic.UValues;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class CloseUI : TaskComponent {
		
		[SerializeReference][SubclassSelector] public UValue<UUI> TargetUUI;
		
		public override void Execute(ITaskable self) {
			UUIManager.instance.Close(TargetUUI.value.gameObject);
		}
	}
}