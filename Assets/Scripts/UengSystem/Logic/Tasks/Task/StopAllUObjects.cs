using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class StopAllUObjects : TaskComponent {
		public override void Execute(ITaskable self) {
			foreach (UObject obj in UObject.Instances) { obj.Stop(); }
		}
	}
}