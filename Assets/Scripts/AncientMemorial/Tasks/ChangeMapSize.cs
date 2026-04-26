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
			MapManager.instance.SetMapSize(newSize, newSpeed, instant);
		}
	}
}