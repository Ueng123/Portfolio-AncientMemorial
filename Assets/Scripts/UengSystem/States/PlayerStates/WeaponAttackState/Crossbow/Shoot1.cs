namespace UengSystem.States.PlayerStates.WeaponAttackState.Crossbow {
	public class Shoot1 : CrossbowAttack {

		public override float attackTime      => 0.6f;
		public override float attackAfterTime => 1.4f;

		public override void OnRoutine() {
			if (step == 0) {

				Shoot(1.3f);
				Kick(0.1f);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(1)) {
				player.state = GetDefaultState();
			}
		}
	}
}