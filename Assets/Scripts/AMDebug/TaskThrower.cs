using AncientMemorial.AMObjects;
using AncientMemorial.Tasks;
using UnityEngine;

namespace AncientMemorial.DebugHelper {
	public class TaskThrower : Manager<TaskThrower> {

		[SerializeReference] [SubclassSelector]
		public Task task;

		public AMObject self;
		public bool     execute;


		public override void ManagerUpdate() {
			if (!execute) return;
			execute = false;
			
			task.Execute((ITaskable)self??this);
		}

		public override void ManagerFixedUpdate() { }
	}
}