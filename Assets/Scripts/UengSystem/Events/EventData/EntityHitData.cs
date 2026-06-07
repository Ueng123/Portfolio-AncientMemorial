using AncientMemorial.Entities;
using AncientMemorial.Projectiles;
using UnityEngine;

namespace UengSystem.Events {
	public record EntityHitData(Entity attackedEntity, Projectile attackedProjectile, Entity reciever, float damage, Vector2? pushDir = null) : EventData();
}