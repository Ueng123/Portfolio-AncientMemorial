using System;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class TaskListItem {
		public float time;
		[SerializeReference] [SubclassSelector]
		public TaskComponent[]  tasks;
	}
}