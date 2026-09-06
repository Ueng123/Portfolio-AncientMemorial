using System;
using AncientMemorial.Entities;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class GetProjectiles : TaskComponent {
		public                                        GameObject      TargetObject;
		[SerializeReference][SubclassSelector] public UValue<Vector2> position;
		public bool PlayEffect = true;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		[SerializeReference][SubclassSelector] public UValue<float>  damage;
		[SerializeReference][SubclassSelector] public UValue<Entity> owner;

		
		public override void Execute(ITaskable self) {
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"                       +
					  $" > ID = {ID.value}\n"                    + 
					  $" > Category = {Category.value}\n"        + 
					  $" > targetPrefab = {TargetObject.name}\n" + 
					  $" > position = {position}\n"              + 
					  $" > PlayEffect = {PlayEffect}\n"       + 
					  $"");
			
			UObject.Get(TargetObject.name, position.value, PlayEffect, Obj => {
				Projectile Projectile = (Projectile)Obj;
				Projectile.owner = owner.value;
				Projectile.damage = damage.value;
				if (!string.IsNullOrWhiteSpace(ID?.value)) Projectile.ID = ID.value;
				if (!string.IsNullOrWhiteSpace(Category?.value)) Projectile.Category = Category.value;
			});
		}
	}
}
