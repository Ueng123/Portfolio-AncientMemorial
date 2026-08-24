using System;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class TaskListItem {
		public float time;
		[SerializeReference] [SubclassSelector]
		public TaskComponent[]  tasks;
	}
}