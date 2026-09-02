namespace UengSystem.Events {
	public interface IEventAgent {
		public void SendEvent(EventType type, EventPriority layer, IEventData data);
	}	
}