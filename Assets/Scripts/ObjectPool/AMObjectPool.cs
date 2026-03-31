using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace AncientMemorial.ObjectPool {
	public class AMObjectPool : Manager<AMObjectPool> {
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
							
							IObjectPoolable script = obj.GetComponent<IObjectPoolable>();
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
			GameObject obj = pools[key].Get();
			IObjectPoolable script = obj.GetComponent<IObjectPoolable>();

			obj.transform.position = position;
			
			script.Get(time);
			obj.SetActive(true);

			if (time == 0) return obj;
			
			GameObject spawnFX = Get("SpawnEffectHelper", position);
			spawnFX.GetComponent<SpawnEffectHelper>().t_s = time;
			
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