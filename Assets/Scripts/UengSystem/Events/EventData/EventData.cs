using System;

using UengSystem.Objects;

namespace UengSystem.Events {
	public interface IEventData { }

	public interface ILifetimeEventData : IEventData {
		bool isValid { get; }
	}

	// A null optional participant is valid; a destroyed or recycled participant is not.
	internal readonly struct EventObjectLifetime {
		private readonly UObject Target;
		public long lifeNumber { get; }

		public EventObjectLifetime(UObject Target) {
			this.Target = Target;
			lifeNumber = Target ? Target.lifeNumber : 0;
		}

		public bool isValid => ReferenceEquals(Target, null) ||
			(Target && Target.lifeNumber == lifeNumber && !Target.isReleased && !Target.lifeCycle.isShuttingDown);

		public bool Matches(UObject Other) => ReferenceEquals(Target, Other) && isValid;
	}
}
