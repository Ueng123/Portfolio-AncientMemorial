using System;
using UengSystem.Utility;

namespace UengSystem.Events {
	public record Event(EventType type, IEventAgent sender, EventData data) {
		public T GetData<T>() where T : EventData => (T)data;
	}
}