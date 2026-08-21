namespace UengSystem.Events {
	public enum EventType {
		Entity_Hit,
		
		Entity_Player_Jump,
		Entity_Player_Land,
		Entity_Player_PrimaryAttack,
		Entity_Player_SecondaryAttack,
		Entity_Player_MoveStart,
		Entity_Player_MoveStop,
		Entity_Player_Dash,
		
		Interact_Start,
		Interact_Cancel,
		Interact_Target,
		Interact_Untarget,
		Interact_Stop,
	}
}