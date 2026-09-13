namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Crossbow {
	public class FallShoot : CrossbowAttack {

		// 정적 프로퍼티
		private const int   shootNum    = 3;
		private const float shootDamage = 2f;
		private const float angleOffset = 40f;

		private const float startOffset = angleOffset * -0.5f;
		private const float offsetStage = angleOffset / shootNum;

		// 인스턴스 프로퍼티
		public override float attackTime       => 0.2f;
		public override float attackAfterTime  => 1;
		public override float attackSpeed => 1f;

		// 오버라이드 메서드
		public override void OnRoutine() {
			if (step == 0 && isProgress(0)) {
				for (int i = 0; i < shootNum-1; i ++) {
					player.animator.SetBool(Attacking, true);
					
					ShootWithoutEffect(shootDamage, startOffset + offsetStage * i);
				}

				Shoot(shootDamage, startOffset + offsetStage * (shootNum - 1));
				Kick(0.6f);
				step = 1;
			}

			if (step == 1 && isProgress(1)) {
				player.entityState = GetDefaultState();
			}
		}
	}
}