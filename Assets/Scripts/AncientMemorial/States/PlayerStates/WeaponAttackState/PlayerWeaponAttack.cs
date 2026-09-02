using UengSystem.States.EnemyStates;
using UengSystem.States.PlayerStates.WeaponAttackState.Crossbow;
using UengSystem.States.PlayerStates.WeaponAttackState.Shield;
using UengSystem.States.PlayerStates.WeaponAttackState.Sword;

namespace UengSystem.States.PlayerStates.WeaponAttackState {
	public abstract class PlayerWeaponAttack : PlayerState, IAttackState {

		public abstract float attackTime       { get; }
		public abstract float attackAfterTime  { get; }
		public abstract float attackSpeed { get; }

		// DAGGER ATTACK
		public static Slash1     slash1     = new ();
		public static Slash2     slash2     = new ();
		public static Slash3     slash3     = new ();
		public static FlashSlash flashSlash = new (); // Skill

		// SHIELD ATTACK
		public static Sweep      sweep      = new ();
		public static Strike     strike     = new ();
		public static FallStrike fallStrike = new (); // FallAttack
		public static ShieldJump shieldJump = new ();  // Skill
		
		// CROSSBOW ATTACK
		public static Shoot1     shoot1     = new ();
		public static Shoot2     shoot2     = new ();
		public static Shoot3     shoot3     = new ();
		public static Shoot5     shoot5     = new ();
		public static FallShoot  fallShoot  = new (); // FallAttack
		public static ShootBoost shootBoost = new ();  // Skill

		 public float GetCooldown() => (attackTime + attackAfterTime) / attackSpeed;
		 public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		 public bool  isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));
		 
		protected int step;

		public override void OnEnter() {
			step = 0;
		}
		
		public override void OnExit() { }
	}
}
