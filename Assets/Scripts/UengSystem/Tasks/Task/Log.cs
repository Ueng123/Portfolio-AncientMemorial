using System;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UnityEngine;

namespace UengSystem.Tasks {
	[Serializable]
	public class Log : TaskComponent {
		
		[SerializeReference][SubclassSelector]
		public UValue<string> text;
		
		public override void Execute(ITaskable self) {
			Debug.Log(text.getValue);
		}
	}
}