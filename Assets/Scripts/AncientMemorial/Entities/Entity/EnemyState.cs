namespace AncientMemorial.Entities {
	public enum EnemyState {
		Wander, // no aggro entity
		Alert, // there's aggro entity but attack cooldown
		AttackReady, // preparing to attack
		Attack, // attacking
		Stun,
		
		None, // 연출같은거 할때 멈춰두는거
	}
}