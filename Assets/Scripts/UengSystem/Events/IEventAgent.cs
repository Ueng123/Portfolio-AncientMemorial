using System.Collections.Generic;
using UengSystem.Events;

namespace UengSystem.Events {
	public interface IEventAgent {
		public void SendEvent(EventType type, int layer, EventData data);
		public void OnEvent(Event e);
	}	
}