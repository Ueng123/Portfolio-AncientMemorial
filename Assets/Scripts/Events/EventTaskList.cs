using System;
using System.Collections.Generic;
using AncientMemorial.Entities;
using AncientMemorial.Events.EventDatas;
using AncientMemorial.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Events {
	[Serializable]
	public class EventTaskList {
		public static List<EventTaskList> eventTaskList = new List<EventTaskList>();
		
		public bool isActivated = false;
		
		[SerializeReference] public TaskList        task;
		[SerializeReference] public EventData       targetEventData;
		public                      EventDetectType DetectType;
	}
}