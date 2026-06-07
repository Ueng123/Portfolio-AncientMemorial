using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Entities {
	[Serializable]
	public class EntityData {
		public string                    name;
		public int                       hp;
		public float                     moveSpeed;
		public float                     jumpPower;
		public float                     attackCooldown;
		public float                     attackSpeed;
		public int                       attackDamage;
		public float                     invincibleTime;
		public float                     groundBoxOffsetX;
		public float                     groundBoxOffsetY;
		public float                     groundBoxSizeX;
		public float                     groundBoxSizeY;
		public float                     aggroThreshold;
		public Dictionary<string, float> aggroCoefficient;
	}
}