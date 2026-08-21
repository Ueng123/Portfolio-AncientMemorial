using System;
using UengSystem.Utility;

namespace UengSystem.Events {
	public static class EventUtility {
		public static void AddListener(this EventType type, Action<Event> func) {
			EventManager.instance.AddListener(type, func);
		}
		
		public static void RemoveListener(this EventType type, Action<Event> func) {
			EventManager.instance.RemoveListener(type, func);
		}
	}
}