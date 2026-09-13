using System;
using AncientMemorial.Entities;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SetEntityFloatStat : TaskComponent {

		// 인스턴스 프로퍼티
		[SerializeReference] [SubclassSelector]
		public UValue<Entity> targetEntity;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> targetValue;
		
		public EntityDataType entityDataType;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			switch (entityDataType) {
				case EntityDataType.MAXHP:
					targetEntity.value.data.HP = (int)targetValue.value;
					break;
				case EntityDataType.HP:
					targetEntity.value.stat.HP = (int)targetValue.value;
					break;
				case EntityDataType.MoveSpeed:
					targetEntity.value.stat.moveSpeed = targetValue.value;
					break;
				case EntityDataType.JumpPower:
					targetEntity.value.stat.jumpPower = targetValue.value;
					break;
				case EntityDataType.AttackSpeed:
					targetEntity.value.stat.attackSpeed = targetValue.value;
					break;
				case EntityDataType.AttackDamage:
					targetEntity.value.stat.attackDamage = targetValue.value;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}