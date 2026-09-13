using System;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ResumeGame : TaskComponent {

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			GameManager.SetTimeScale(1);
		}
	}
}