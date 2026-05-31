using System;
using AncientMemorial.Map;
using UengSystem.Logic.Tasks;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ChangeMapSize : TaskComponent {
		public Vector2 newSize;
		public float   newSpeed;
		public bool    instant;
		
		public override void Execute(ITaskable self) {
			string speed = instant ? "Instant" : newSize.ToString();
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data"              +
					  $" > target size = {newSize}\n" +
					  $" > speed = {speed}");
			
			MapManager.instance.SetMapSize(newSize, newSpeed, instant);
		}
	}
}