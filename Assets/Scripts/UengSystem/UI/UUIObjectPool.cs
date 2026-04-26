using System.Collections.Generic;
using System.Linq;
using AncientMemorial.Objects;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace UengSystem.UI {
	public class UUIObjectPool : Manager<UUIObjectPool> {
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
						
						UUI uiScript = obj.GetComponent<UUI>();
						uiScript.OnGet();
						
						obj.SetActive(false);
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
			Debug.Log($"[UI OPEN] canvas : {canvas.gameObject.name}, key : {key}");
			
			GameObject      obj    = pools[key].Get();
			UUI script = obj.GetComponent<UUI>();

			obj.transform.SetParent(canvas.transform);

			float openTime = prefabData.FirstOrDefault((data) => data.prefab.name == obj.name)!.openTime;
			
			script.Get(openTime);
			script.OnOpen();
			
			obj.SetActive(true);
			
			return obj;
		}

		public void Close(GameObject obj, bool finalRelease = false) {
			Debug.Log($"[UI CLOSE] canvas : {obj.transform.parent.name}, key : {obj.name}, finalRelease : {finalRelease}");
			if (!finalRelease) pools[obj.name].Release(obj);

			UUI script = obj.GetComponent<UUI>();

			float closeTime = prefabData.FirstOrDefault((data) => data.prefab.name == obj.name)!.closeTime;
			
			script.Release(finalRelease?-1:closeTime);

			if (!finalRelease) return;

			script.OnClose();
			obj.SetActive(false);
			obj.transform.SetParent(roots[obj.name]);
		}
		
		public override void ManagerUpdate() {  }
		public override void ManagerFixedUpdate() {  }
	}
}