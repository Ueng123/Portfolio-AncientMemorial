using AncientMemorial.Events.EventDatas;

namespace AncientMemorial.Events {
	public record Event(EventType type, IEventAgent sender, EventData data);
}