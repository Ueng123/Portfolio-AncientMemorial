using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.PlayerStates.WeaponAttackState.Crossbow;
using AncientMemorial.States.PlayerStates.WeaponAttackState.Shield;
using AncientMemorial.States.PlayerStates.WeaponAttackState.Sword;

namespace AncientMemorial.States.PlayerStates.WeaponAttackState {
	public abstract class PlayerWeaponAttack : PlayerState, IAttackState {

		// 정적 프로퍼티
		public static Slash1     slash1     = new ();
		public static Slash2     slash2     = new ();
		public static Slash3     slash3     = new ();
		public static FlashSlash flashSlash = new (); // Skill

		
		public static Sweep      sweep      = new ();
		public static Strike     strike     = new ();
		public static FallStrike fallStrike = new (); // FallAttack
		public static ShieldJump shieldJump = new ();  // Skill
		
		
		public static Shoot1     shoot1     = new ();
		public static Shoot2     shoot2     = new ();
		public static Shoot3     shoot3     = new ();
		public static Shoot5     shoot5     = new ();
		public static FallShoot  fallShoot  = new (); // FallAttack
		public static ShootBoost shootBoost = new ();  // Skill

		// 인스턴스 프로퍼티
		public abstract float attackTime       { get; }
		public abstract float attackAfterTime  { get; }
		public abstract float attackSpeed { get; }
		 
		protected int step;

		 // 인스턴스 메서드
		 public float GetCooldown() => (attackTime + attackAfterTime) / attackSpeed;
		 public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		 public bool  isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));

		// 오버라이드 메서드
		public override void OnEnter() {
			step = 0;
		}
		
		public override void OnExit() { }
	}
}
