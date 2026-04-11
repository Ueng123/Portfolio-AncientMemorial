using System.Collections.Generic;
using System.Linq;
using AncientMemorial.Objects;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace UengSystem.UI {
	public class UUIManager : Manager<UUIManager> {
		[FormerlySerializedAs("prefabs")] [SerializeField]
		private UUIPrefabItem[]                            prefabData;
		private Dictionary<string, ObjectPool<GameObject>> pools;
		private Dictionary<string, Transform>              roots;

		public override void Initialize() {
			pools = new Dictionary<string, ObjectPool<GameObject>>();
			roots = new Dictionary<string, Transform>             ();
			
			foreach (UUIPrefabItem data in prefabData) {
				Transform newRoot  = new GameObject($"{data.prefab.name} root").transform;
				roots[data.prefab.name] = newRoot;
				newRoot.parent     = transform;
				
				pools[data.prefab.name] = new ObjectPool<GameObject>(
					createFunc: () => {
						GameObject obj = Instantiate(data.prefab, newRoot);
						obj.name = data.prefab.name;
							
						IObjectPoolable script = obj.GetComponent<IObjectPoolable>();
						script.OnFirstGet();
							
						return obj;
					},
					actionOnGet: (obj) => {  },
					actionOnRelease: (obj) => {  },
					actionOnDestroy: Destroy,
					collectionCheck: true,
					defaultCapacity: 1,
					maxSize: 100
				); 
			}
		}

		public GameObject Open(string key, UCanvas canvas) {
			GameObject      obj    = pools[key].Get();
			IObjectPoolable script = obj.GetComponent<IObjectPoolable>();

			obj.transform.SetParent(canvas.transform);

			float openTime = prefabData.FirstOrDefault((data) => data.prefab.name == obj.name)!.openTime;
			
			script.Get(openTime);
			obj.SetActive(true);
			
			return obj;
		}

		public void Close(GameObject obj, bool finalRelease = false) {
			IObjectPoolable script = obj.GetComponent<IObjectPoolable>();

			float closeTime = prefabData.FirstOrDefault((data) => data.prefab.name == obj.name)!.closeTime;
			
			script.Release((finalRelease)?-1:closeTime);

			if (!finalRelease) return;

			obj.SetActive(false);
			
			obj.transform.SetParent(roots[obj.name]);
			pools[obj.name].Release(obj);
		}
		
		public override void ManagerUpdate() {  }
		public override void ManagerFixedUpdate() {  }
	}
}