using System;

using UengSystem.Objects;

namespace UengSystem.Events {
	public interface IEventData { }

	public interface ILifetimeEventData : IEventData {

		// 인스턴스 프로퍼티
		bool isValid { get; }
	}

	// A null optional participant is valid; a destroyed or recycled participant is not.
	public readonly struct EventObjectLifetime {

		// 인스턴스 프로퍼티
		private readonly UObject Target;
		public long lifeNumber { get; }

		public bool isValid => ReferenceEquals(Target, null) ||
			(Target && Target.lifeNumber == lifeNumber && !Target.isReleased && !Target.lifeCycle.isShuttingDown);

		// 인스턴스 메서드
		public EventObjectLifetime(UObject Target) {
			this.Target = Target;
			lifeNumber = Target ? Target.lifeNumber : 0;
		}

		public bool Matches(UObject Other) => ReferenceEquals(Target, Other) && isValid;
	}
}
