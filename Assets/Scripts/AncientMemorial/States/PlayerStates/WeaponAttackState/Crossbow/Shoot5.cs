namespace AncientMemorial.States.PlayerStates.WeaponAttackState.Crossbow {
	public class Shoot5 : CrossbowAttack {
		public override float attackTime      => 1f;
		public override float attackAfterTime => 2f;
		
		public override void OnRoutine() {
			if (step == 0 && isProgress(0)) {

				Shoot(0.8f);
				Kick(0.05f);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(0.2f)) {

				Shoot(0.8f);
				Kick(0.05f);
				
				step = 2;
			}
			
			if (step == 2 && isProgress(0.4f)) {

				Shoot(0.8f);
				Kick(0.05f);
				
				step = 3;
			}
			
			if (step == 3 && isProgress(0.6f)) {

				Shoot(0.8f);
				Kick(0.05f);
				
				step = 4;
			}
			
			if (step == 4 && isProgress(0.8f)) {

				Shoot(0.8f);
				Kick(0.1f);
				
				step = 5;
			}
			
			if (step == 5 && isProgress(1)) {
				player.entityState = GetDefaultState();
			}
		}
	}
}