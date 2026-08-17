using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UnityEngine;

namespace AncientMemorial.Tasks {
	public class SpawnWaveEnemy : TaskComponent {
		public                                        GameObject      TargetObject;
		[SerializeReference][SubclassSelector] public UValue<Vector2> position;
		[SerializeReference][SubclassSelector] public UValue<float>   spawnTime;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) { 
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"                                            +
					  (ID       != null ? $" > ID = {ID.value}\n" : "")               +
					  (Category != null ? $" > Category = {Category.value}\n" : "")   +
					  $" > targetPrefab = {TargetObject.name}\n"                      +
					  $" > position = {position.value}\n"                             +
					  (spawnTime != null ? $" > duration = {spawnTime.value}\n" : "") +
					  $"");
			
			GameObject obj  = UObjectPool.instance.Get(TargetObject.name, position.value, spawnTime?.value??0);
			UObject    uObj = obj.GetComponent<UObject>();

			if (ID       !=null &&ID.value       !="") uObj.ID       = ID.value;
			if (Category !=null &&Category.value !="") uObj.Category = Category.value;
		}
	}
}