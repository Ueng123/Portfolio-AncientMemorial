using UengSystem.Logic.Tasks;

namespace UengSystem.Objects {
	public class TaskObject : BasicUObject {
		public Task task;
		
		public override void Initialize() {
			base.Initialize();
			task.Execute(this);
		}
	}
}