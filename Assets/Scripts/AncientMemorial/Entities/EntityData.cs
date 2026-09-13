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

		// 정적 프로퍼티
		private static Dictionary<EntityType, EntityData> entityDataCache;

		// 인스턴스 프로퍼티
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

		// 정적 메서드
		public static bool GetInitialized() {
			return entityDataCache != null;
		}
		
		public static void Initialize() {
			entityDataCache = new Dictionary<EntityType, EntityData>();
			foreach (EntityType entityType in Enum.GetValues(typeof(EntityType))) {
				entityDataCache[entityType] = UJSON<EntityData>.LoadData($"EntityData/{entityType}.json");
			}
		}

		// 인스턴스 메서드
		public EntityData(EntityType type) {
			if (entityDataCache==null) throw new NullReferenceException("u need to do InitializeEntityDataCache()");
			if (!entityDataCache.TryGetValue(type, out EntityData data)) throw new KeyNotFoundException("WTF?");
			this = data;
		}
	}
}