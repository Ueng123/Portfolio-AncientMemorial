using System.Collections.Generic;
using AncientMemorial.Events.EventDatas;

namespace AncientMemorial.Events {
	public interface IEventAgent {
		public void SendEvent(EventType type, EventData data);
		public void OnEvent(Event e);
	}	
}