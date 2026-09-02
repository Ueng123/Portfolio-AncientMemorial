using System;
using AncientMemorial.Entities;
using AncientMemorial.Projectiles;
using UnityEngine;

namespace UengSystem.Events {
	[Serializable]
	public struct HitData : IEventData {
		public Entity     attackedEntity;
		public Projectile attackedProjectile;
		public Entity     reciever;
		public float      damage;
		public Vector2?   pushDir;
		public bool       ignoreInvincible;
		
		public HitData(Entity attackedEntity, Projectile attackedProjectile, Entity reciever, float damage, Vector2? pushDir = null, bool ignoreInvincible = false) {
			this.attackedEntity     = attackedEntity;
			this.attackedProjectile = attackedProjectile;
			this.reciever           = reciever;
			this.damage             = damage;
			this.pushDir            = pushDir;
			this.ignoreInvincible   = ignoreInvincible;
		}
	}
}