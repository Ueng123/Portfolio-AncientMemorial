using System.Collections.Generic;
using System.Linq;
using AncientMemorial.Objects;
using JetBrains.Annotations;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.UI.UUIQueues;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace UengSystem.UI {
	public class UUIObjectPool : Manager<UUIObjectPool> {
		[SerializeField]
		public UUIPrefabItem[] prefabData;
		private readonly Dictionary<string, ObjectPool<GameObject>> pools    = new ();
		private readonly Dictionary<string, Transform>              roots    = new ();
		private readonly Dictionary<string, UUIQueue>               uuiQueue = new ();
		
		public override void Initialize() {
			pools.Clear();
			roots.Clear();
			uuiQueue.Clear();
			
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
			Debug.Log("[UUI] UUI OPENNING");
			
			GameObject obj = pools[key].Get();
			UUI script = obj.GetComponent<UUI>();

			obj.transform.SetParent(canvas.transform);

			float openTime = prefabData.FirstOrDefault((data) => data.prefab.name == obj.name)!.openTime;
			
			script.Get(openTime);
			script.canvas = canvas;
			script.OnOpen();
			
			obj.SetActive(true);

			script.isReleased = false;
			return obj;
		}
		
		public void Close(GameObject obj, bool finalRelease = false) {
			UUI script = obj.GetComponent<UUI>();

			float closeTime = prefabData.FirstOrDefault((data) => data.prefab.name == obj.name)!.closeTime;
			
			script.Release(finalRelease?-1:closeTime);

			if (!finalRelease) return;
			Debug.Log("[UUI] UUI CLOSED");
			
			script.OnClose();
			obj.SetActive(false);
			obj.transform.SetParent(roots[obj.name]);
			
			script.isReleased = true;
			pools[obj.name].Release(obj);
		}

		public void AddUUIQueue(string key, UUIQueueItem queueItem) {
			if (!uuiQueue.ContainsKey(key)) uuiQueue.Add(key, new UUIQueue());
			uuiQueue[key].queue.Enqueue(queueItem);
		}
		
		public override void ManagerUpdate() {
			foreach (string key in uuiQueue.Keys) {
				uuiQueue[key].GetNextQueue(key);
			}
		}

		public override void ManagerFixedUpdate() {
			
		}
	}
}