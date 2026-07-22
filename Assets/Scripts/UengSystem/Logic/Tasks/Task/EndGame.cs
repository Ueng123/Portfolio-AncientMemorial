using System;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class EndGame : TaskComponent {
		public override void Execute(ITaskable self) {
			Application.Quit();
			
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
	}
}