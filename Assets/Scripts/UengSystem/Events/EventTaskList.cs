using System;
using System.Collections.Generic;
using UengSystem.Events.EventDatas;
using UengSystem.Logic.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Events {
	[Serializable]
	public class EventTaskList {
		public static List<EventTaskList> eventTaskList = new List<EventTaskList>();
		
		public bool isActivated = false;
		
		[SerializeReference] public Task        task;
		[SerializeReference] public EventData       targetEventData;
		public                      EventDetectType DetectType;
	}
}