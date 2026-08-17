namespace UengSystem.States.PlayerStates.WeaponAttackState.Crossbow {
	public class Shoot2 : CrossbowAttack {
		public override float attackTime      => 0.9f;
		public override float attackAfterTime => 1.35f;
		
		public override void OnRoutine() {
			if (step == 0 && isProgress(0)) {

				Shoot(1f);
				Kick(0.05f);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(0.5f)) {

				Shoot(1f);
				Kick(0.1f);
				
				step = 2;
			}
			
			if (step == 2 && isProgress(1)) {
				player.state = GetDefaultState();
			}
		}
	}
}