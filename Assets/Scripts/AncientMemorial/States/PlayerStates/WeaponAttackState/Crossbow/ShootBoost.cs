namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Crossbow {
	public class ShootBoost : CrossbowAttack {

		// 인스턴스 프로퍼티
		public override float attackTime       => 0f;
		public override float attackAfterTime  => 10f;
		public override float attackSpeed => 1f;

		// 오버라이드 메서드
		public override void OnEnter() {
			base.OnEnter();
			skillTimer.Tick();
			player.entityState = GetDefaultState();
		}
	}
}