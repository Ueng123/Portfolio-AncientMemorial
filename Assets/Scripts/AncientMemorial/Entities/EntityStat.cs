using System;

namespace AncientMemorial.Entities {
	[Serializable]
	public class EntityStat {
		public float HP;
		public float Energy;
		public float Defense;
		public float MoveSpeed;
		public float JumpPower;
		public float DamageMultiplier;
		public float AttackSpeed;

		public EntityStat(float HP, float Energy, float Defense, float MoveSpeed, float JumpPower, float DamageMultiplier, float AttackSpeed) {
			this.HP               = HP;
			this.Energy           = Energy;
			this.Defense          = Defense;
			this.MoveSpeed        = MoveSpeed;
			this.JumpPower        = JumpPower;
			this.DamageMultiplier = DamageMultiplier;
			this.AttackSpeed      = AttackSpeed;
		}
		
		public EntityStat newInstance() {
			return new EntityStat(
				HP,
				Energy,
				Defense,
				MoveSpeed,
				JumpPower,
				DamageMultiplier,
				AttackSpeed
			);
		}
	}
}