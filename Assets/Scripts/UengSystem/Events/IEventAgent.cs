namespace UengSystem.Events {
	public interface IEventAgent {
		public void SendEvent(EventType type, int layer, EventData data);
	}	
}