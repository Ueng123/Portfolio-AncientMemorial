namespace UengSystem.Events {
	public interface IEventAgent {

		// 인스턴스 메서드
		public void SendEvent(EventType type, EventPriority layer, IEventData data);
	}	
}