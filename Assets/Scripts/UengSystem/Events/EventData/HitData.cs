using System;
using AncientMemorial.Entities;
using AncientMemorial.Projectiles;
using UnityEngine;

namespace UengSystem.Events {
	[Serializable]
	public struct HitData : ILifetimeEventData {
		public Entity     attackedEntity;
		public Projectile attackedProjectile;
		public Entity     reciever;
		public float      damage;
		public Vector2?   pushDir;
		public bool       ignoreInvincible;
		// 대상이 피해 처리와 OnHit을 마친 뒤 호출함. 무적/무효 타격에는 호출하지 않음.
		public Action onHit { get; set; }
		private readonly EventObjectLifetime EntityLifetime;
		private readonly EventObjectLifetime ProjectileLifetime;
		private readonly EventObjectLifetime ReceiverLifetime;
		private readonly EventObjectLifetime AttackerLifetime;
		public Entity attacker { get; }
		public long attackedEntityLifeNumber => EntityLifetime.lifeNumber;
		public long attackedProjectileLifeNumber => ProjectileLifetime.lifeNumber;
		public long recieverLifeNumber => ReceiverLifetime.lifeNumber;
		public long attackerLifeNumber => AttackerLifetime.lifeNumber;
		public bool isValid => reciever && ReceiverLifetime.Matches(reciever)
			&& EntityLifetime.Matches(attackedEntity) && ProjectileLifetime.Matches(attackedProjectile)
			&& AttackerLifetime.Matches(attacker);
		
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
			EntityLifetime     = new EventObjectLifetime(attackedEntity);
			ProjectileLifetime = new EventObjectLifetime(attackedProjectile);
			ReceiverLifetime   = new EventObjectLifetime(reciever);
			AttackerLifetime   = new EventObjectLifetime(Attacker);
		}
	}
}
