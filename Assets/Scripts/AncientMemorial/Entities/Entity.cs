using System;
using System.Linq;
using AncientMemorial.Buffs;
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
		public BuffList   buffList;

		public  bool    isGround;
		private Vector2 groundBoxOffset;
		private Vector2 groundBoxSize;
		
		// Static Methods //
		
		
		
		// Instance Methods //

		protected abstract void Death();
		
		// ETC. Override //

		public override void Initialize() {
			entityData = DataStorage.instance.Entities
									.FirstOrDefault(e => e.entityType == entityType);
			
			if (entityData == null) { throw new NullReferenceException("NO ENTITY DATA BRUH ;;;"); }
			
			entityStat      = entityData.baseStat.newInstance();
			groundBoxOffset = entityData.groundBoxOffset;
			groundBoxSize   = entityData.groundBoxSize;
			
			buffList = new BuffList();
			
			base.Initialize();
		}

		public override void Uninitialize() {
			entityStat = null;
			entityData = default;
			
			// 버프 초기화
			buffList.RemoveAllBuffs();
			
			base.Uninitialize();
		}

		// UPDATE ROUTINE //
		
		protected override void EarlyRoutine() {
			// BUFF
			buffList.Routine();
			
			// ARTIFACT
			
		}

		public override void OnEvent(Events_Event e) {
			buffList.OnEvent(e);
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