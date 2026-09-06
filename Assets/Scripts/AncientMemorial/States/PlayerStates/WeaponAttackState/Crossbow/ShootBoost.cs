namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Crossbow {
	public class ShootBoost : CrossbowAttack {
		public override float attackTime       => 0f;
		public override float attackAfterTime  => 10f;
		public override float attackSpeed => 1f;

		public override void OnEnter() {
			base.OnEnter();
			skillTimer.Tick();
			player.entityState = GetDefaultState();
		}
	}
}