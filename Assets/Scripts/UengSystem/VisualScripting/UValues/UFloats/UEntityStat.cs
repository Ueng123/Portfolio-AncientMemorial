using System;
using AncientMemorial.Entities;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class UEntityStat : UValue<float> {
		protected override bool  getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<Entity> targetEntity;
		public EntityDataType type;

		protected override float getValue {
			get {
				if (!targetEntity.value) {
					DebugManager.Log("NO TARGET ENTITY");
					return 0;
				}
				
				return type switch {
					EntityDataType.MAXHP          => targetEntity.value.entityData.HP,
					EntityDataType.HP             => targetEntity.value.entityStat.HP,
					EntityDataType.MoveSpeed      => targetEntity.value.entityStat.moveSpeed,
					EntityDataType.JumpPower      => targetEntity.value.entityStat.jumpPower,
					EntityDataType.AttackSpeed    => targetEntity.value.entityStat.attackSpeed,
					EntityDataType.AttackDamage   => targetEntity.value.entityStat.attackDamage,
					EntityDataType.AttackCooldown => targetEntity.value.entityData.attackCooldown,
					_                             => throw new ArgumentOutOfRangeException()
				};
			}
		}
	}
}