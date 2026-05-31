using System;
using UnityEngine;

namespace AncientMemorial.Entities {
	[Serializable]
	public record EntityData {
		public EntityType entityType;
		public string     name;
		public float      hp;
		public float      moveSpeed;
		public float      jumpPower;
		public float      attackSpeed;
		public float      attackDamage;
		public float      groundBoxOffsetX;
		public float      groundBoxOffsetY;
		public float      groundBoxSizeX;
		public float      groundBoxSizeY;
	}
}