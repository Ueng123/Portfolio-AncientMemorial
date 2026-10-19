using System;
using UengSystem.Utility;
using UengSystem.Objects;

namespace UengSystem.Events {
	public class Event {
		public EventType   type;
		public IEventAgent sender;
		public IEventData  data;

		// 인스턴스 프로퍼티
		private readonly EventValid SenderLifetime;

		// 생성자
		public Event(EventType type, IEventAgent sender, IEventData data) {
			this.type = type;
			this.sender = sender;
			this.data = data;
			SenderLifetime = new EventValid(sender.As<UObject>());
		}

		private bool isValidSender => SenderLifetime.Match(sender.As<UObject>());
		private bool isValidData   => data?.isValid??true;
		public  bool isValid       => isValidSender && isValidData;

		// 인스턴스 메서드
		public T GetData<T>() where T : IEventData => data.To<T>();
	}
}
