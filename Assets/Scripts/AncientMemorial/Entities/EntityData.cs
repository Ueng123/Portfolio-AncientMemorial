using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Entities {
	[Serializable]
	public record EntityData {
		public string Name;
		public float  HP;
		public float  MoveSpeed;
		public float  JumpPower;
		public float  AttackSpeed;
		public float  AttackDamage;
		public float  GroundBoxOffsetX;
		public float  GroundBoxOffsetY;
		public float  GroundBoxSizeX;
		public float  GroundBoxSizeY;
	}
}