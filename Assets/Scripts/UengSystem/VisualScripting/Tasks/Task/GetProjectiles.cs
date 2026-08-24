using System;
using AncientMemorial.Entities;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class GetProjectiles : TaskComponent {
		public                                        GameObject      TargetObject;
		[SerializeReference][SubclassSelector] public UValue<Vector2> position;
		[SerializeReference][SubclassSelector] public UValue<float>   spawnTime;
		
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
					  $" > duration = {spawnTime.value}\n"       + 
					  $"");
			
			GameObject obj  = UObjectPool.instance.Get(TargetObject.name, position.value, spawnTime.value);
			Projectile proj = obj.GetComponent<Projectile>();
			
			proj.owner  = owner.value;
			proj.damage = damage.value;

			if (ID       !=null) proj.ID       = ID.value;
			if (Category !=null) proj.Category = Category.value;
		}
	}
}