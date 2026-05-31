using System;
using System.IO;
using System.Linq;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;

namespace AncientMemorial.Entities {
	public abstract class Entity : AdvancedUObject {

		public static Player player;
		
		public EntityType entityType;
		public EntityData entityData;
		public Team       team;
		
		public EntityStat entityStat;

		public  bool    isGround;
		private Vector2 groundBoxOffset;
		private Vector2 groundBoxSize;
		
		// Static Methods //
		
		
		
		// Instance Methods //

		protected abstract void Death();
		
		// ETC. Override //
		
		public override void Initialize() {
			string path = Path.Combine(Application.streamingAssetsPath, $"EntityData/{entityType}.json");
			if (!File.Exists(path)) {
				Debug.LogError($"NO FILE FOUND : [{entityType}] BRO;((((");
				return;
			}
			string jsonText = File.ReadAllText(path);
			
			entityData = JsonUtility.FromJson<EntityData>(jsonText);

			name = entityData.Name;
			entityStat      = new EntityStat(
				entityData.HP,
				entityData.MoveSpeed,
				entityData.JumpPower,
				entityData.AttackSpeed,
				entityData.AttackDamage
			);
			groundBoxOffset = new Vector2(
				entityData.GroundBoxOffsetX,
				entityData.GroundBoxOffsetY
			);
			groundBoxSize   = new Vector2(
				entityData.GroundBoxSizeX,
				entityData.GroundBoxSizeY
			);
			
			base.Initialize();
		}

		public override void Uninitialize() {
			entityStat = null;
			entityData = default;
			
			base.Uninitialize();
		}

		// UPDATE ROUTINE //
		
		protected override void EarlyRoutine() {
		}

		public override void OnEvent(Events_Event e) {
		}

		protected override void LateRoutine() {
			if (entityStat.HP < 0) {
				Death();
			}
		}
		
		// FIXED UPDATE ROUTINE //
		
		protected override void FixedRoutine() {
			isGround = (bool)Physics2D.OverlapBoxAll(
				point: (Vector2)transform.position + groundBoxOffset,
				size:  groundBoxSize,
				angle: 0
				).FirstOrDefault(c => c.GetComponent<Standable>()!=null);
		}
	}
}