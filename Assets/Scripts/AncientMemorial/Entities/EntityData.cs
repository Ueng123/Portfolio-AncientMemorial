using System;
using UnityEngine;

namespace AncientMemorial.Entities {
	[Serializable]
	public record EntityData {
		public EntityType entityType;
		public string     name;
		public EntityStat baseStat;
		public Vector2    groundBoxOffset;
		public Vector2    groundBoxSize;
	}
}