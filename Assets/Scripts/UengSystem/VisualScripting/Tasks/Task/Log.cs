using System;
using UengSystem.UDebug;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class Log : TaskComponent {
		
		[SerializeReference][SubclassSelector]
		public UValue<string> text;
		
		public override void Execute(ITaskable self) {
			DebugManager.Log(text.value);
		}
	}
}