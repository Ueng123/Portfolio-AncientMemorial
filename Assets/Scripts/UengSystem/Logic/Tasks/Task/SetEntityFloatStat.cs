using System;
using AncientMemorial.Entities;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class SetEntityFloatStat : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<Entity> targetEntity;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> targetValue;
		
		public EntityDataType entityDataType;
		
		public override void Execute(ITaskable self) {
			switch (entityDataType) {
				case EntityDataType.MAXHP:
					targetEntity.value.entityData.hp = (int)targetValue.value;
					break;
				case EntityDataType.HP:
					targetEntity.value.entityStat.hp = (int)targetValue.value;
					break;
				case EntityDataType.MoveSpeed:
					targetEntity.value.entityStat.moveSpeed = targetValue.value;
					break;
				case EntityDataType.JumpPower:
					targetEntity.value.entityStat.jumpPower = targetValue.value;
					break;
				case EntityDataType.AttackSpeed:
					targetEntity.value.entityStat.attackSpeed = targetValue.value;
					break;
				case EntityDataType.AttackDamage:
					targetEntity.value.entityStat.attackDamage = targetValue.value;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}