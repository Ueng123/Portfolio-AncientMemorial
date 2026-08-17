using System;

namespace UengSystem.Events {
	[Serializable]
	public record EventValueData<T>(T value):EventData();
}