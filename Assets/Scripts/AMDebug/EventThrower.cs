using AncientMemorial.AMObjects;
using AncientMemorial.Events;
using AncientMemorial.Events.EventDatas;
using UnityEngine;
using Event = AncientMemorial.Events.Event;
using EventType = AncientMemorial.Events.EventType;

namespace AncientMemorial.DebugHelper {
	public class EventThrower : Manager<EventThrower> {
		public EventType   eventType;
		public AMObject    sender;
		[SerializeReference][SubclassSelector]
		public EventData   eventData;

		public bool send;


		public override void ManagerUpdate() {
			if (!send) return;
			send = false;
			
			if (eventData == null || sender) return;
			Event e = new (eventType, sender, eventData);
			EventManager.instance.AddEvent(e);
		}

		public override void ManagerFixedUpdate() { }
	}
}