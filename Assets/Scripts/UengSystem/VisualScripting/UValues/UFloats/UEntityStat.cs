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
					EntityDataType.MAXHP          => targetEntity.value.data.HP,
					EntityDataType.HP             => targetEntity.value.stat.HP,
					EntityDataType.MoveSpeed      => targetEntity.value.stat.moveSpeed,
					EntityDataType.JumpPower      => targetEntity.value.stat.jumpPower,
					EntityDataType.AttackSpeed    => targetEntity.value.stat.attackSpeed,
					EntityDataType.AttackDamage   => targetEntity.value.stat.attackDamage,
					EntityDataType.AttackCooldown => targetEntity.value.data.attackCooldown,
					_                             => throw new ArgumentOutOfRangeException()
				};
			}
		}
	}
}