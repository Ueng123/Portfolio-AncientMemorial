namespace UengSystem.Objects.LifeCycle {
	public abstract class Getting : LifeCycleState {

		// 인스턴스 프로퍼티
		protected override float duration => target.usingGettingDuration;

		// 인스턴스 메서드
		protected Getting(UObject Target) : base(Target) { }
	}
}
