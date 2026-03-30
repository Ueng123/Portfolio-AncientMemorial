using System;
using AncientMemorial.AMObjects;
using AncientMemorial.ObjectPool;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class GetObject : Task {
		public GameObject TargetObject;
		public Vector2    position;
		public float      spawnTime;

		public string ID;
		public string Category;
		
		public override void Execute(ITaskable self) {
			GameObject obj = AMObjectPool.instance.Get(TargetObject.name, position, spawnTime);
			AMObject amobj = obj.GetComponent<AMObject>();

			amobj.ID = ID;
			amobj.Category = Category;
		}
	}
}