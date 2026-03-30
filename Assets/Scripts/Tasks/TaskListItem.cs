using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class TaskListItem {
		public float time;
		[FormerlySerializedAs("task")] [SerializeReference] [SubclassSelector]
		public Task[]  tasks;
	}
}