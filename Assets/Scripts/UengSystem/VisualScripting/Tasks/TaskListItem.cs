using System;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class TaskListItem {
		// 인스턴스 프로퍼티
		public float time;
		[SerializeReference] [SubclassSelector]
		public TaskComponent[]  tasks;
	}
}