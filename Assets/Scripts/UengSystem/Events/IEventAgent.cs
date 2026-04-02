using System.Collections.Generic;
using UengSystem.Events.EventDatas;

namespace UengSystem.Events {
	public interface IEventAgent {
		public void SendEvent(EventType type, EventData data);
		public void OnEvent(Event e);
	}	
}