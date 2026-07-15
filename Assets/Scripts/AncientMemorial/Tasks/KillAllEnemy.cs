using System;
using AncientMemorial.Entities;
using UengSystem.Logic.Tasks;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class KillAllEntity : TaskComponent {
		public bool exceptPlayer;
		
		public override void Execute(ITaskable self) {
			foreach (Entity e in Entity.entities) {
				if (e == Entity.player && exceptPlayer) continue;
				e.entityStat.hp = 0;
			}
		}
	}
}