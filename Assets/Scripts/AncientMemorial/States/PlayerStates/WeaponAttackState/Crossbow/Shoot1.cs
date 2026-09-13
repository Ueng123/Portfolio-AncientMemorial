namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Crossbow {
	public class Shoot1 : CrossbowAttack {

		// 인스턴스 프로퍼티
		public override float attackTime      => 0.6f;
		public override float attackAfterTime => 1.4f;

		// 오버라이드 메서드
		public override void OnRoutine() {
			if (step == 0) {

				Shoot(2f);
				Kick(0.1f);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(1)) {
				player.entityState = GetDefaultState();
			}
		}
	}
}