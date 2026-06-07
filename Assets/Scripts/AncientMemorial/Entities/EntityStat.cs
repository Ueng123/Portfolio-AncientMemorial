using System;

namespace AncientMemorial.Entities {
	[Serializable]
	public class EntityStat {
		public float hp;
		public float moveSpeed;
		public float jumpPower;
		public float attackSpeed;
		public float attackDamage;

		public EntityStat(float hp, float moveSpeed, float jumpPower, float attackSpeed, float attackDamage) {
			this.hp           = hp;
			this.moveSpeed    = moveSpeed;
			this.jumpPower    = jumpPower;
			this.attackSpeed  = attackSpeed;
			this.attackDamage = attackDamage;
		}
	}
}