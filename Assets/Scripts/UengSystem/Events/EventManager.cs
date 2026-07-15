using System;
using System.Collections.Generic;
using UnityEngine;
using UengSystem.Managers;

namespace UengSystem.Events {
	public class EventManager : Manager<EventManager> {
		
		[Header("EventManager")]	
		public List<List<Event>> events = new List<List<Event>>();
		public List<Event> GetEvents(int layer) => events[layer];
		
		public void AddEvent(Event e, int layer) {
			if (e.sender         == null) throw new ArgumentException("event must have sender lil bro 🥀🥀", nameof(e));
			int currSize = events.Count;
			if (currSize - 1 < layer) { for (int i = 0; i < layer - currSize + 1; i++) events.Add(new List<Event>());}

			if (!events[layer].Contains(e)) events[layer].Add(e);
		}

		public void RemoveEvent(Event e, int layer) {
			if (!events[layer].Contains(e)) throw new InvalidOperationException("event does not exist twin 🤞");
			if (events.Count - 1 < layer) throw new InvalidOperationException("even LAYER doesn't exist ma bro 🥀🥀");
			events[layer].Remove(e);
		}

		public void RemoveAllEvents() {
			foreach (List<Event> l in events) {
				l.Clear();
			}
		}

		public override void ManagerUpdate() {  }

		public override void ManagerFixedUpdate() {  }
	}
}