using UengSystem.Objects;
using System.Collections.Generic;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public class CrystalPhase2State : CrystalState {
		private static readonly int               ATTACKING          = "attacking".GetHash();
		private                 List<AttackArea> MissileAttackAreas = new List<AttackArea>();
		
		// -1 : <- | 0 : v | 1 : ->
		protected void ShootHugeMissile(float where) {
			Vector2 mapSize = MapManager.instance.GetTargetMapSize(); 

			Vector2 hitboxPos  = new (mapSize.x*where /3f, (mapSize.y +1));
			Vector2 hitboxSize = new (mapSize.x       /3f, (mapSize.y +1)*2);
			
			ShootHugeMissile(hitboxPos, hitboxSize);
		}
		
		protected void ShootHugeMissile(Vector2 hitboxPos, Vector2 hitboxSize) {
			Vector2 mapSize = MapManager.instance.GetTargetMapSize();
			
			GameObject      obj = UObject.Get("CrystalHugeMissile", new Vector2(hitboxPos.x, mapSize.y-2), PlayEffect: false);
			BasicProjectile hugeMissile = obj.GetComponent<BasicProjectile>();
			hugeMissile.Category = "crystalMissile";
			
			float      timeToFall = Mathf.Sqrt(20 * (obj.transform.position.y-0.5f) / Physics2D.gravity.y*-1);

			MissileAttackAreas.Add(Entity.AttackArea(crystal, Random.Range(90, 100), timeToFall, hitboxPos, hitboxSize));
		}

		protected override void ClearMissiles() {
			base.ClearMissiles();
			foreach (AttackArea awareObject in MissileAttackAreas) {
				awareObject.Cancel();
			}
		}

		public override void OnEnter() {
			base.OnEnter();
			crystal.animator.SetBool(ATTACKING, true);
		}
		
		public override void OnExit() {
			base.OnExit();
			crystal.animator.SetBool(ATTACKING, false);
		}
	}
}
