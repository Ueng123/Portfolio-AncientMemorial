using System;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class EndGame : TaskComponent {

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			Application.Quit();
			
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
	}
}