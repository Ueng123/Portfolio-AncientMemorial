using UengSystem.Objects;

namespace UengSystem.Events {
	public readonly struct EventObjectLifetime : IEventData {

		// 인스턴스 프로퍼티
		private readonly UObject Target;
		public           long    lifeNumber { get; }

		private bool isValidObject => Target && Target.Matches(lifeNumber) && !Target.isNotWorking;
		public  bool isValid       => ReferenceEquals(Target, null) || isValidObject;

		// 인스턴스 메서드
		public EventObjectLifetime(UObject Target) {
			this.Target = Target;
			lifeNumber  = Target ? Target.lifeNumber : 0;
		}

		public bool Matches(UObject Other) => ReferenceEquals(Target, Other) && isValid;
	}
}
