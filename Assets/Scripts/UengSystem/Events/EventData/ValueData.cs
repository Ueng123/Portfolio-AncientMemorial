using System;

namespace UengSystem.Events {
	[Serializable]
	public record ValueData<T>(T value):EventData();
}