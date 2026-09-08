using System;
using UengSystem.Events;
using UengSystem.Objects;

namespace UengSystem.Events {
	[Serializable]
	public struct ValueData<T> : ILifetimeEventData {
		public T value;
		private readonly EventObjectLifetime ValueLifetime;
		public long valueLifeNumber => ValueLifetime.lifeNumber;
		public bool isValid => ValueLifetime.Matches(value is UObject Target ? Target : null);
		
		public ValueData(T value) {
			this.value = value;
			ValueLifetime = new EventObjectLifetime(value is UObject Target ? Target : null);
		}
	}
}
