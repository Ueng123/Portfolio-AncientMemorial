namespace UengSystem.Events {
	public enum EventPriority {
		Cancel = 0,
		
		Stop = 1, // Interaction Stop
		Remove = 1,
		
		Start = 2, // Interaction Start
		Add = 2,
		
		Action = 3, // Jump, Primary/Secondary Attack, Dash, etc. 
		State = 3, // Move
		
		Hit = 4,
	}
}