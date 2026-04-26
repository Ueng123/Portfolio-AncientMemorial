using System;
using System.Collections.Generic;

using AncientMemorial.Objects;
using UengSystem.Managers;

using UnityEngine;
using UnityEngine.Pool;

namespace UengSystem.ObjectPool {
	public class UObjectPool : Manager<UObjectPool> {
		[SerializeField]
		private GameObject[]                               prefabs;
		private Dictionary<string, ObjectPool<GameObject>> pools;
		private Dictionary<string, Transform>              roots;

		public override void Initialize() {
			pools = new Dictionary<string, ObjectPool<GameObject>>();
			roots = new Dictionary<string, Transform>             ();
			
			foreach (GameObject prefab in prefabs) {
				Transform newRoot  = new GameObject($"{prefab.name} root").transform;
				roots[prefab.name] = newRoot;
				newRoot.parent     = transform;
				
				pools[prefab.name] = new ObjectPool<GameObject>(
						createFunc: () => {
							GameObject obj = Instantiate(prefab, newRoot);
							obj.name = prefab.name;
							obj.SetActive(false);
							
							IObjectPoolable script = obj.GetComponent<IObjectPoolable>();
							script.gettable = true;
							script.OnFirstGet();
							
							return obj;
						},
						actionOnGet: (obj) => {  },
						actionOnRelease: (obj) => {  },
						actionOnDestroy: (obj) => {
							Destroy(obj);
						},
						collectionCheck: true,
						defaultCapacity: 0,
						maxSize: 100
				); 
			}
		}
		
		public GameObject Get(string key, Vector2 position, float time = 0) {
			List<GameObject> willReleaseObjects = new();
			GameObject       obj                = null;
			IObjectPoolable  script             = null;

			int i = 0;
			while (i++<=100) {
				obj = pools[key].Get();
				script = obj.GetComponent<IObjectPoolable>();
				if (script.gettable) {
					break;
				}
				
				willReleaseObjects.Add(obj);
			}

			if (!obj) throw new Exception();
			
			foreach (GameObject releaseObj in willReleaseObjects) {
				pools[key].Release(releaseObj);
			}

			obj.transform.position = position;
			
			script.Get(time);
			obj.SetActive(true);
			
			return obj;
		}

		public void Release(GameObject obj, float time = 0) {
			IObjectPoolable script = obj.GetComponent<IObjectPoolable>();

			script.Release(time);

			if (time > 0) return;

			obj.SetActive(false);
			
			obj.transform.SetParent(roots[obj.name]);
			pools[obj.name].Release(obj);
		}
		
		public override void ManagerUpdate()      { }
		public override void ManagerFixedUpdate() { }
	}
}