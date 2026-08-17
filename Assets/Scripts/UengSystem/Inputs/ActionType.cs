namespace UengSystem.Inputs {
	public enum ActionType {
		// MOUSE //
		MousePosition         = 0000,
		MouseLClick           = 0001,
		MouseRClick           = 0002,
		MouseMClick           = 0003,
		
		// DEBUG //
		OpenDebugUI            = 1000,
		DebugMode2            = 1001,
		DebugMode3            = 1002,
		DebugMode4            = 1003,
		DebugMode5            = 1004,
		DebugMode6            = 1005,
		Debug_HealthUp        = 1006,
		Debug_HealthDown      = 1007,
		Debug_MaxHealthUp     = 1008,
		Debug_MaxHealthDown   = 1009,
		Debug_DamageUp        = 1010,
		Debug_DamageDown      = 1011,
		Debug_MoveSpeedUp     = 1012,
		Debug_MoveSpeedDown   = 1013,
		Debug_AttackSpeedUp   = 1014,
		Debug_AttackSpeedDown = 1015,
		Debug_Damage          = 1016,
		Debug_AddHealPotion   = 1017,
		
		// UI //
		Enter                 = 2000, 
		Quit                  = 2001,
		
		// PLAYER //
		Jump                  = 3000,
		Move                  = 3001,
		Dash                  = 3002,
		Interact              = 3003,
		Heal                  = 3004,
	}
}