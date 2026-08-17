using System;
using UengSystem.Logic.UValues;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class Log : TaskComponent {
		
		[SerializeReference][SubclassSelector]
		public UValue<string> text;
		
		public override void Execute(ITaskable self) {
			DebugManager.Log(text.value);
		}
	}
}