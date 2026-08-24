using System.Collections.Generic;
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
							
							IObjectPoolable script = obj.GetComponent<IObjectPoolable>();
							script.OnFirstGet();
							
							obj.SetActive(false);
							return obj;
						},
						actionOnGet: (obj) => {  },
						actionOnRelease: (obj) => {  },
						actionOnDestroy: Destroy,
						collectionCheck: true,
						defaultCapacity: 10,
						maxSize: 50
				); 
			}
		}
		
		public GameObject Get(string key, Vector2 position, float time = 0) {
			GameObject       obj                = pools[key].Get();
			IObjectPoolable  script             = obj.GetComponent<IObjectPoolable>();
			
			obj.transform.position = position;
			
			obj.SetActive(true);
			script.Get(time);
			
			script.isReleased = false;
			return obj;
		}

		public void Release(GameObject obj, float time = 0) {
			IObjectPoolable script = obj.GetComponent<IObjectPoolable>();
			if (script.isReleased) return;
			
			script.Release(time);
			if (time > 0) return;
			
			script.isReleased = true;
			obj.SetActive(false);
			
			obj.transform.SetParent(roots[obj.name]);
			pools[obj.name].Release(obj);
		}
	}
}