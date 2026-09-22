using UnityEngine;

namespace UengSystem.Objects.LifeCycle {
	public class RoutineEntry {
		// 인스턴스 프로퍼티
		public readonly UObject Target;
		public readonly long    Life;
		public readonly long    Execution;
		public readonly int     Frame;
			
		public bool isActive => Target && Target.isActive && Target.Matches(Life) && Time.frameCount > Frame;
			
		public bool isLifeCycle => Target && Target.Matches(Life)
										  && Target.lifeCycle.IsCurrent(Target.lifeCycle.currentState, Execution);

		// 인스턴스 메서드
		public RoutineEntry(UObject Target, long Execution = 0) {
			this.Target    = Target;
			Life           = Target.lifeNumber;
			this.Execution = Execution;
			Frame          = Time.frameCount;
		}
	}
}