using System;
using UnityEngine.Serialization;

namespace AncientMemorial.Entities {
	[Serializable]
	public struct EntityStat {

		// 인스턴스 프로퍼티
		[FormerlySerializedAs("hp")] public float HP;
		public                              float moveSpeed;
		public                              float jumpPower;
		public                              float attackSpeed;
		public                              float attackDamage;

		// 인스턴스 메서드
		public EntityStat(float hp, float moveSpeed, float jumpPower, float attackSpeed, float attackDamage) {
			this.HP           = hp;
			this.moveSpeed    = moveSpeed;
			this.jumpPower    = jumpPower;
			this.attackSpeed  = attackSpeed;
			this.attackDamage = attackDamage;
		}
	}
}