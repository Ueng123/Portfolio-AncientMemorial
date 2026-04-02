using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.AMDebug {
	public class TaskThrower : Manager<TaskThrower> {

		[FormerlySerializedAs("task")] [SerializeReference] [SubclassSelector]
		public TaskComponent taskComponent;

		public UObject self;
		public bool     execute;


		public override void ManagerUpdate() {
			if (!execute) return;
			execute = false;
			
			taskComponent.Execute((ITaskable)self??this);
		}

		public override void ManagerFixedUpdate() { }
	}
}