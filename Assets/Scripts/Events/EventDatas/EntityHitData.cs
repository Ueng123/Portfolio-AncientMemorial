using AncientMemorial.Entities;
using AncientMemorial.Projectiles;

namespace AncientMemorial.Events.EventDatas {
	public record EntityHitData(Entity attacker, Entity reciever, Projectile projectile = null) : EventData();
}