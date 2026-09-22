using System;
using UengSystem.Utility;
using UengSystem.Objects;

namespace UengSystem.Events {
	public record Event(EventType type, IEventAgent sender, IEventData data) {

		// 인스턴스 프로퍼티
		private readonly EventObjectLifetime SenderLifetime = new(sender.As<UObject>());
		public bool isValid => SenderLifetime.Matches(sender.As<UObject>()) 
							   && (data != null || data.To<IEventData>().isValid);

		// 인스턴스 메서드
		public T GetData<T>() where T : IEventData => data.To<T>();
	}
}
