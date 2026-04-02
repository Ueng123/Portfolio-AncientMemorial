using AncientMemorial.Entities;
using AncientMemorial.Projectiles;

namespace UengSystem.Events.EventDatas {
	public record EntityHitData(Entity attacker, Entity reciever, Projectile projectile = null) : EventData();
}