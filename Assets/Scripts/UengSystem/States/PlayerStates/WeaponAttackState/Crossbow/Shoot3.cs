namespace UengSystem.States.PlayerStates.WeaponAttackState.Crossbow {
	public class Shoot3 : CrossbowAttack {
		public override float attackTime      => 1.25f;
		public override float attackAfterTime => 0.75f;
		
		public override void OnRoutine() {
			if (step == 0 && isProgress(0f)) {

				Shoot(0.8f);
				Kick(0.05f);
				
				step = 1;
			}
			
			if (step == 1 && isProgress(0.333f)) {

				Shoot(0.8f);
				Kick(0.05f);
				
				step = 2;
			}
			
			if (step == 2 && isProgress(0.666f)) {

				Shoot(0.8f);
				Kick(0.1f);
				
				step = 3;
			}
			
			if (step == 3 && isProgress(1)) {
				player.state = GetDefaultState();
			}
		}
	}
}