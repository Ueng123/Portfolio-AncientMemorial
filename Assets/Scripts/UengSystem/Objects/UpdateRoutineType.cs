namespace UengSystem.Objects {
	[System.Flags]
	public enum UpdateRoutineType {
		Processing,
		EarlyRoutine = 0b_0000_0001,
		Routine      = 0b_0000_0010,
		EventRoutine = 0b_0000_0100,
		LateRoutine  = 0b_0000_1000,
		LifeCycleRoutine = 0b_0001_0000,
		RoutineEnd
	}
}
