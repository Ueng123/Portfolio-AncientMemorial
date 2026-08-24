using System;
using System.Linq;
using AncientMemorial.Entities;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class KillAllEntity : TaskComponent {
		public bool exceptPlayer;
		public bool exceptFriendly;

		public EntityType[]   targetTypes;
		[SerializeReference] [SubclassSelector]
		public UValue<string> targetCategory;
		[SerializeReference] [SubclassSelector]
		public UValue<Entity> targetEntity;
		
		public override void Execute(ITaskable self) {
			for (int i = 0; i < Entity.entities.Count; i++) {
				Entity e = Entity.entities[i];
				
				if (e == Entity.player     && exceptPlayer) continue;
				if (e.Friendly             && exceptFriendly) continue;
				if (targetTypes    != null && targetTypes.Length != 0 && !targetTypes.Contains(e.entityType)) continue;
				if (targetCategory != null && e.Category         != targetCategory.value) continue;
				if (targetEntity   != null && e                  != targetEntity.value) continue;
				e.entityStat.HP = 0;
			}
		}
	}
}