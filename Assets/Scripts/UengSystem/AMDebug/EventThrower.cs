using UengSystem.Events;
using UengSystem.Events.EventDatas;
using UengSystem.Managers;
using UengSystem.Objects;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;
using Events_EventType = UengSystem.Events.EventType;

namespace UengSystem.AMDebug {
	public class EventThrower : Manager<EventThrower> {
		public Events_EventType   eventType;
		public UObject    sender;
		[SerializeReference][SubclassSelector]
		public EventData   eventData;

		public bool send;


		public override void ManagerUpdate() {
			if (!send) return;
			send = false;
			
			if (eventData == null || sender) return;
			Events_Event e = new (eventType, sender, eventData);
			EventManager.instance.AddEvent(e);
		}

		public override void ManagerFixedUpdate() { }
	}
}