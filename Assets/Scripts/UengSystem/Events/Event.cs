using System;
using UengSystem.Utility;
using UengSystem.Objects;

namespace UengSystem.Events {
	public record Event(EventType type, IEventAgent sender, IEventData data) {
		private readonly EventObjectLifetime SenderLifetime = new(sender as UObject);
		public long senderLifeNumber => SenderLifetime.lifeNumber;
		public bool isValid => SenderLifetime.Matches(sender as UObject)
			&& (data is not ILifetimeEventData LifetimeData || LifetimeData.isValid);

		public T GetData<T>() where T : IEventData => (T)data;
	}
}
