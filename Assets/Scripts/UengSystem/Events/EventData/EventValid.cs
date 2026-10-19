using UengSystem.Objects;

namespace UengSystem.Events {
	public readonly struct EventValid {

		// 인스턴스 프로퍼티
		private readonly UObject Target;
		public           int    lifeNumber { get; }

		private bool isValidObject => Target && Target.Matches(lifeNumber) && !Target.OutGame;
		public  bool isValid       => ReferenceEquals(Target, null) || isValidObject;

		// 인스턴스 메서드
		public EventValid(UObject Target) {
			this.Target = Target;
			lifeNumber  = Target?.lifeNumber??0;
		}

		public bool Match(UObject Other) => ReferenceEquals(Target, Other) && isValid;
	}
}
