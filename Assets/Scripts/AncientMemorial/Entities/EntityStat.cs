using System;

namespace AncientMemorial.Entities {
	[Serializable]
	public class EntityStat {
		public float hp = 0f;
		public float moveSpeed = 0f;
		public float jumpPower = 0f;
		public float attackSpeed = 0f;
		public float attackDamage = 0f;

		public EntityStat(float hp, float moveSpeed, float jumpPower, float attackSpeed, float attackDamage) {
			this.hp           = hp;
			this.moveSpeed    = moveSpeed;
			this.jumpPower    = jumpPower;
			this.attackSpeed  = attackSpeed;
			this.attackDamage = attackDamage;
		}
	}
}