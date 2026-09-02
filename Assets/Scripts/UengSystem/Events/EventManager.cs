using System;
using System.Collections.Generic;
using UnityEngine;
using UengSystem.Managers;
using UengSystem.Utility;

namespace UengSystem.Events {
	public class EventManager : Manager<EventManager> {
		
		[Header("EventManager")]	
		public readonly List<List<Event>> events = new (6);
		public List<Event> GetEvents(int layer) => events[layer];

		private Dictionary<EventType, BufferedList<Action<Event>>> EventActions = new ();

		public override void Initialize() {
			base.Initialize();
			for (int i = 0; i < 6; i++) {
				events.Add(new List<Event>());
			}
		}

		public void AddListener(EventType type, Action<Event> func) {
			EventActions.TryAdd(type, new BufferedList<Action<Event>>(20, 5));
			EventActions[type].Add(func);
		}

		public void RemoveListener(EventType type, Action<Event> func) {
			EventActions[type].Remove(func);
		}

		public void AddEvent(Event e, EventPriority layer) {
			int layerN = (int)layer;
			
			if (e.sender == null) throw new ArgumentException("event must have sender", nameof(e));
			int currSize = events.Count;
			if (currSize - 1 < layerN) { for (int i = 0; i < layerN - currSize + 1; i++) events.Add(new List<Event>(10));}

			if (!events[layerN].Contains(e)) events[layerN].Add(e);
		}

		public void RemoveAllEvents() {
			foreach (List<Event> eventList in events) {
				eventList.Clear();
			}
		}

		public void EventRoutine() {
			foreach (List<Event> eventList in events) {
				foreach (Event e in eventList) {
					if (!EventActions.TryGetValue(e.type, out BufferedList<Action<Event>> actions)) return;
					actions.Apply();
					for (int i = 0; i < actions.Count; i++) {
						Action<Event> action = actions[i];
						action.Invoke(e);
					}
				}
			}
		}
	}
}