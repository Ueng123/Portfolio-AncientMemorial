using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace AncientMemorial.Entities {
	[Serializable]
	public struct EntityData {
		public string                    name;
		public float                       hp;
		public float                     moveSpeed;
		public float                     jumpPower;
		public float                     attackCooldown;
		public float                     attackSpeed;
		public float                       attackDamage;
		public float                     invincibleTime;
		public float                     groundBoxOffsetX;
		public float                     groundBoxOffsetY;
		public float                     groundBoxSizeX;
		public float                     groundBoxSizeY;
		public float                     aggroThreshold;
		public Dictionary<string, float> aggroCoefficient;


		private static Dictionary<EntityType, EntityData> entityDataCache;

		public static bool GetInitialized() {
			return entityDataCache != null;
		}
		
		public static void Initialize() {
			entityDataCache = new Dictionary<EntityType, EntityData>();
			foreach (EntityType entityType in Enum.GetValues(typeof(EntityType))) {
				string path = Path.Combine(Application.streamingAssetsPath, $"EntityData/{entityType}.json");
				if (!File.Exists(path)) { throw new FileNotFoundException($"NO FILE FOUND : [{entityType}] BRO;(((("); }
			
				string jsonText = File.ReadAllText(path);
			
				entityDataCache[entityType] = JsonConvert.DeserializeObject<EntityData>(jsonText);
			}
		}
		
		public EntityData(EntityType type) {
			if (entityDataCache==null) throw new NullReferenceException("u need to do InitializeEntityDataCache()");
			if (!entityDataCache.TryGetValue(type, out EntityData data)) throw new KeyNotFoundException($"WTF?");
			this = data;
		}
	}
}