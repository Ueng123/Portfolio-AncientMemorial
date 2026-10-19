using System;
using AncientMemorial.Entities;
using AncientMemorial.Projectiles;
using UnityEngine;

namespace UengSystem.Events {
	[Serializable]
	public struct HitData : IEventData {
		
		public Entity     attackedEntity;
		public Projectile attackedProjectile;
		public Entity     attacker;
		
		public Entity     reciever;
		
		public float      damage;
		public Vector2?   pushDir;
		public bool       ignoreInvincible;


		public Action onHit;
		
		private readonly EventValid EntityLifetime;
		private readonly EventValid ProjectileLifetime;
		private readonly EventValid ReceiverLifetime;
		private readonly EventValid AttackerLifetime;
		
		
		public bool isValid => reciever 
							   && ReceiverLifetime.Match(reciever)
							   && EntityLifetime.Match(attackedEntity)
							   && ProjectileLifetime.Match(attackedProjectile) 
							   && AttackerLifetime.Match(attacker);

		// 인스턴스 메서드
		public HitData(Entity attackedEntity, Projectile attackedProjectile, Entity reciever, float damage, Vector2? pushDir = null, bool ignoreInvincible = false) {
			this.attackedEntity     = attackedEntity;
			this.attackedProjectile = attackedProjectile;
			this.reciever           = reciever;
			this.damage             = damage;
			this.pushDir            = pushDir;
			this.ignoreInvincible   = ignoreInvincible;
			onHit = null;
			
			Entity Attacker = attackedEntity ? attackedEntity : attackedProjectile ? attackedProjectile.owner : null;
			attacker           = Attacker;
			
			EntityLifetime     = new EventValid(attackedEntity);
			ProjectileLifetime = new EventValid(attackedProjectile);
			ReceiverLifetime   = new EventValid(reciever);
			AttackerLifetime   = new EventValid(Attacker);
		}
	}
}
