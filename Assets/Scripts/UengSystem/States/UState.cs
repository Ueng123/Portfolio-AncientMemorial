using UengSystem.Utility;

namespace UengSystem.States {
	public abstract class UState {

		// 인스턴스 프로퍼티
		public StopWatch stateTimer = new ();

		// 인스턴스 메서드
		public void Enter() {
			stateTimer.Tick();
			OnEnter();
		}

		public abstract void OnEnter();

		public abstract void OnEarlyRoutine();
		public abstract void OnRoutine();
		public abstract void OnLateRoutine();
		public abstract void OnFixedRoutine();

		public void Exit() {
			OnExit();
		}
		public abstract void OnExit();
	}
}
