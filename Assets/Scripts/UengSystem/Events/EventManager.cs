using System;
using System.Collections.Generic;
using UnityEngine;
using UengSystem.Managers;

namespace UengSystem.Events {
	public class EventManager : Manager<EventManager> {
		
		[Header("EventManager")]
		public List<Event> events = new List<Event>();
		public List<Event> GetEvents() => events;
		
		public void AddEvent(Event e) {
			if (e.sender == null) throw new ArgumentException("event must have sender lil bro 🥀🥀", "e");
			events.Add(e);
		}

		public void RemoveEvent(Event e) {
			if (!events.Contains(e)) throw new InvalidOperationException("event does not exist twin 🤞");
			events.Remove(e);
		}

		public void RemoveAllEvents() => events.Clear();

		public override void ManagerUpdate() {  }

		public override void ManagerFixedUpdate() {  }
	}
}