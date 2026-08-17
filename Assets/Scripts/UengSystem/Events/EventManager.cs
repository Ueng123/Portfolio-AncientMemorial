using System;
using System.Collections.Generic;
using UnityEngine;
using UengSystem.Managers;

namespace UengSystem.Events {
	public class EventManager : Manager<EventManager> {
		
		[Header("EventManager")]	
		public readonly List<List<Event>> events = new (5);
		public List<Event> GetEvents(int layer) => events[layer];

		private Dictionary<EventType, List<Action<Event>>> EventActions = new ();

		public override void Initialize() {
			base.Initialize();
			for (int i = 0; i < 5; i++) {
				events.Add(new List<Event>());
			}
		}

		public void RegisterEvent(EventType type, Action<Event> func) {
			if (EventActions.TryAdd(type, new List<Action<Event>> { func })) return;
			EventActions[type].Add(func);
		}

		public void UnregisterEvent(EventType type, Action<Event> func) {
			EventActions[type].Remove(func);
		}

		public void AddEvent(Event e, int layer) {
			if (e.sender         == null) throw new ArgumentException("event must have sender lil bro 🥀🥀", nameof(e));
			int currSize = events.Count;
			if (currSize - 1 < layer) { for (int i = 0; i < layer - currSize + 1; i++) events.Add(new List<Event>(10));}

			if (!events[layer].Contains(e)) events[layer].Add(e);
		}

		public void RemoveAllEvents() {
			foreach (List<Event> eventList in events) {
				eventList.Clear();
			}
		}

		public void EventRoutine() {
			foreach (List<Event> eventList in events) {
				foreach (Event e in eventList) {
					if (!EventActions.TryGetValue(e.type, out List<Action<Event>> actions)) return;
					foreach (Action<Event> action in actions) {
						action.Invoke(e);
					}
				}
			}
		}
	}
}