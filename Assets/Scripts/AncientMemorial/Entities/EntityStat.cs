using System;

namespace AncientMemorial.Entities {
	[Serializable]
	public class EntityStat {
		public float HP;
		public float MoveSpeed;
		public float JumpPower;
		public float AttackSpeed;

		public EntityStat(float HP, float MoveSpeed, float JumpPower, float AttackSpeed) {
			this.HP               = HP;
			this.MoveSpeed        = MoveSpeed;
			this.JumpPower        = JumpPower;
			this.AttackSpeed      = AttackSpeed;
		}
	}
}