namespace UengSystem.Events {
	public enum EventType {
		Entity_Behaviour_Jump,
		Entity_Behaviour_PrimaryAttack,
		Entity_Behaviour_SecondaryAttack,
		Entity_Behaviour_Move,
		Entity_Behaviour_Dash,
		Entity_Behaviour_Hit,
		
		Interact_Start,
		Interact_Cancel,
		Interact_Target,
		Interact_Untarget,
		Interact_Stop,
	}
}