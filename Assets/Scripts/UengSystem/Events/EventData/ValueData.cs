using System;
using UengSystem.Events;
using UengSystem.Objects;
using UengSystem.Utility;

namespace UengSystem.Events {
	[Serializable]
	public struct ValueData<T> : IEventData {

		// 인스턴스 프로퍼티
		public T value;
		private readonly EventValid ValueLifetime;
		public int valueLifeNumber => ValueLifetime.lifeNumber;
		public bool isValid => ValueLifetime.Match(value.As<UObject>());

		// 인스턴스 메서드
		public ValueData(T value) {
			this.value = value;
			ValueLifetime = new EventValid(value.As<UObject>());
		}
	}
}
