using UengSystem.Objects;
using UengSystem.Utility;

namespace UengSystem.States {
	public abstract class UState {
		public IStateObject owner;

		public StopWatch stateTimer = new ();
		
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