using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class RoutineEntry {
		// 인스턴스 프로퍼티
		public readonly UObject Target;
		public readonly int    Life;
		public readonly int    Execution;
			
		public bool isActive => Target.isActive && Target.Matches(Life);
		
		private bool isCurrentState => Target.lifeStateMachine.IsCurrent(Target.lifeStateMachine.currentState, Execution);
		public bool isLifeCycle => Target.Matches(Life) && isCurrentState;
		
		// 인스턴스 메서드
		public RoutineEntry(UObject Target, int Execution = 0) {
			this.Target    = Target;
			Life           = Target.lifeNumber;
			this.Execution = Execution;
		}
	}
}