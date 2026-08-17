namespace UengSystem.Events {
	public record Event(EventType type, IEventAgent sender, EventData data);
}