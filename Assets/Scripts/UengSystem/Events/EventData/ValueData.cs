using System;
using UengSystem.Events;
using UengSystem.Objects;

namespace UengSystem.Events {
	[Serializable]
	public struct ValueData<T> : ILifetimeEventData {

		// 인스턴스 프로퍼티
		public T value;
		private readonly EventObjectLifetime ValueLifetime;
		public long valueLifeNumber => ValueLifetime.lifeNumber;
		public bool isValid => ValueLifetime.Matches(value is UObject Target ? Target : null);

		// 인스턴스 메서드
		public ValueData(T value) {
			this.value = value;
			ValueLifetime = new EventObjectLifetime(value is UObject Target ? Target : null);
		}
	}
}
