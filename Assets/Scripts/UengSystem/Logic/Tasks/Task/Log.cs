using System;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class Log : TaskComponent {
		
		[SerializeReference][SubclassSelector]
		public UValue<string> text;
		
		public override void Execute(ITaskable self) {
			Debug.Log(text.getValue);
		}
	}
}