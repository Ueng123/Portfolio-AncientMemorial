using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Tasks {
	[Serializable]
	public class TaskListItem {
		public float time;
		[SerializeReference] [SubclassSelector]
		public TaskComponent[]  tasks;
	}
}