using System;
using UengSystem.Events;

namespace UengSystem.Events {
	[Serializable]
	public struct ValueData<T> : IEventData {
		public T value;
		
		public ValueData(T value) {
			this.value = value;
		}
	}
}