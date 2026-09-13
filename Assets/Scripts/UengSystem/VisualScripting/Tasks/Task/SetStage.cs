using System;
using UengSystem.StageManager;
using UnityEngine.Serialization;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SetStage : TaskComponent {

		// 인스턴스 프로퍼티
		public UScenes uScenes;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			USceneManager.SetStage(uScenes);
		}
	}
}