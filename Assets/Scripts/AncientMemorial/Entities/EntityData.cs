using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientMemorial.Entities {
	[Serializable]
	public struct EntityData {
		public string name;
		public float  HP;
		public float  moveSpeed;
		public float  jumpPower;
		public float  attackCooldown;
		public float  attackSpeed;
		public float  attackDamage;
		public float  invincibleTime;
		public float  groundBoxOffsetX;
		public float  groundBoxOffsetY;
		public float  groundBoxSizeX;
		public float  groundBoxSizeY;

		private static Dictionary<EntityType, EntityData> entityDataCache;

		public static bool GetInitialized() {
			return entityDataCache != null;
		}
		
		public static void Initialize() {
			entityDataCache = new Dictionary<EntityType, EntityData>();
			foreach (EntityType entityType in Enum.GetValues(typeof(EntityType))) {
				entityDataCache[entityType] = UJSON<EntityData>.LoadData($"EntityData/{entityType}.json");
			}
		}
		
		public EntityData(EntityType type) {
			if (entityDataCache==null) throw new NullReferenceException("u need to do InitializeEntityDataCache()");
			if (!entityDataCache.TryGetValue(type, out EntityData data)) throw new KeyNotFoundException("WTF?");
			this = data;
		}
	}
}