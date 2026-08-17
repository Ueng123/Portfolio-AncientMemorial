using System.Collections.Generic;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.UDebug;
using UengSystem.UI.UUIQueues;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace UengSystem.UI {
	public class UUIPool : Manager<UUIPool> {
		[FormerlySerializedAs("UIData")] [FormerlySerializedAs("prefabData")] [SerializeField]
		public GameObject[] prefabList;
		private readonly Dictionary<string, ObjectPool<GameObject>> pools            = new (15);
		private readonly Dictionary<string, Transform>              roots            = new (15);
		private readonly Dictionary<string, UUIQueue>               uuiQueue         = new (5);
		
		public override void Initialize() {
			pools.Clear();
			roots.Clear();
			uuiQueue.Clear();
			
			foreach (GameObject prefab in prefabList) {
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
					defaultCapacity: 1,
					maxSize: 100
				);
			}
		}

		public GameObject Open(string key, UCanvas canvas) {
			DebugManager.Log("[UUI] UUI OPENNING");
			
			GameObject obj = pools[key].Get();
			UUI UI = obj.GetComponent<UUI>();

			obj.transform.SetParent(canvas.transform);
			
			UI.Get(UI.openTime);
			UI.canvas = canvas;
			UI.OnOpen();
			
			obj.SetActive(true);

			UI.isReleased = false;
			return obj;
		}
		
		public void Close(GameObject obj, bool finalRelease = false) {
			UUI UI = obj.GetComponent<UUI>();
			if (UI.isReleased) return;
			
			UI.Release(finalRelease?-1:UI.closeTime);

			if (!finalRelease) return;
			DebugManager.Log("[UUI] UUI CLOSED");
			
			UI.OnClose();
			obj.SetActive(false);
			obj.transform.SetParent(roots[obj.name]);
			
			UI.isReleased = true;
			pools[obj.name].Release(obj);
		}

		public void AddUUIQueue(string key, UUIQueueItem queueItem) {
			uuiQueue.TryAdd(key, new UUIQueue());
			uuiQueue[key].queue.Enqueue(queueItem);
			uuiQueue[key].Next(key);
		}
	}
}