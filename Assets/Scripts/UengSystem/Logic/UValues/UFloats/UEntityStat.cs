using System;
using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class UEntityStat : UValue<float> {
		public override bool  getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<Entity> targetEntity;
		public EntityDataType type;

		public override float getValue {
			get {
				return type switch {
					EntityDataType.HP             => targetEntity.value.entityStat.hp,
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