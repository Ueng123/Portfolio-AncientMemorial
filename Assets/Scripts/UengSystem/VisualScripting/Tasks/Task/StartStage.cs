using System;
using UengSystem.StageManager;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class StartStage : TaskComponent {

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			USceneManager.StartStage();
		}
	}
}