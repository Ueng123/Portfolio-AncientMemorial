namespace AncientMemorial.Entities {
	public enum EnemyState {
		Wander, // no aggro entity
		Alert, // there's aggro entity but attack cooldown
		AttackReady, // preparing to attack
		Attack, // attacking
		Stun,
	}
}