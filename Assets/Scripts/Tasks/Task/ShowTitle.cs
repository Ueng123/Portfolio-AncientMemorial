using System;
using AncientMemorial.UI;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ShowTitle : Task {
		public string title;
		public string mainText;
		public string subText;

		public float speed;
		
		public override void Execute(ITaskable self) {
			UIManager.instance.ScheduleTitle(title, mainText, subText, speed);
		}
	}
}