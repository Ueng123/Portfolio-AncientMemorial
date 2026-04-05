using System;
using UengSystem.Tasks.Logic.String;
using UnityEngine;

namespace UengSystem.Tasks {
	[Serializable]
	public class Log : TaskComponent {
		public UString text;
		
		public override void Execute(ITaskable self) {
			Debug.Log(text.GetText());
		}
	}
}