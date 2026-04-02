using System;
using UengSystem.Tasks;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ShowTitle : TaskComponent {
		public string title;
		public string mainText;
		public string subText;

		public float speed;
		
		public override void Execute(ITaskable self) {
			// UIManager.instance.ScheduleTitle(title, mainText, subText, speed);
		}
	}
}