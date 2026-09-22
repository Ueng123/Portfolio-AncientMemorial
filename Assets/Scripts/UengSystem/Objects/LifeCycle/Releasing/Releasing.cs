namespace UengSystem.Objects.LifeCycle {
	public abstract class Releasing : LifeCycleState {

		// 인스턴스 프로퍼티
		protected override float duration => target.usingReleasingDuration;

		// 인스턴스 메서드
		protected Releasing(UObject Target) : base(Target) { }
	}
}
