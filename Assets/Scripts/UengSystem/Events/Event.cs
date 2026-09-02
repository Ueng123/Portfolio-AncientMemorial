using System;
using UengSystem.Utility;

namespace UengSystem.Events {
	public record Event(EventType type, IEventAgent sender, IEventData data) {
		public T GetData<T>() where T : IEventData => (T)data;
	}
}