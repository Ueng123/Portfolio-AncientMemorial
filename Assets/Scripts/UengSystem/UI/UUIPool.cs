using System.Collections.Generic;
using UengSystem.Managers;
using System;
using UengSystem.Objects;
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
		private readonly HashSet<UUI> Owned = new();
		private bool ShuttingDown;
		
		public override void Initialize() {
			if (pools.Count != 0) throw new InvalidOperationException("UUIPool is already initialized.");
			var Keys = new HashSet<string>(StringComparer.Ordinal);
			foreach (GameObject Prefab in prefabList) {
				if (!Prefab || !Prefab.GetComponent<UUI>() || !Keys.Add(Prefab.name))
					throw new InvalidOperationException("UI prefab keys must be unique UUI prefab names.");
			}
			pools.Clear();
			roots.Clear();
			uuiQueue.Clear();
			
			foreach (GameObject prefab in prefabList) {
				Transform newRoot  = new GameObject($"{prefab.name} root").transform;
				roots[prefab.name] = newRoot;
				newRoot.parent     = transform;
				newRoot.gameObject.SetActive(false);
				
				pools[prefab.name] = new ObjectPool<GameObject>(
					createFunc: () => {
						GameObject obj = Instantiate(prefab, newRoot);
						obj.name = prefab.name;
							
						obj.SetActive(false);
						UUI Target = obj.GetComponent<UUI>();
						Target.OnFirstGet();
						string Key = prefab.name;
						Target.BindPool(Item => ReturnToPool(Key, (UUI)Item));
						Owned.Add(Target);
						return obj;
					},
					actionOnGet: (obj) => {  },
					actionOnRelease: (obj) => {  },
					actionOnDestroy: Obj => { Owned.Remove(Obj.GetComponent<UUI>()); Destroy(Obj); },
					collectionCheck: true,
					defaultCapacity: 1,
					maxSize: 100
				);
			}
		}

		internal UUI Acquire(string PrefabKey, UCanvas Canvas) {
			if (ShuttingDown) throw new InvalidOperationException("UUIPool is shutting down.");
			if (!Canvas) throw new ArgumentNullException(nameof(Canvas));
			if (PrefabKey == null || !pools.TryGetValue(PrefabKey, out ObjectPool<GameObject> Pool))
				throw new KeyNotFoundException("Unregistered UI prefab: " + PrefabKey);
			UUI Target = Pool.Get().GetComponent<UUI>();
			Target.transform.SetParent(Canvas.transform, false);
			Target.canvas = Canvas;
			return Target;
		}
		
		private void ReturnToPool(string Key, UUI Target) {
			if (ShuttingDown || Target.lifeCycle.isShuttingDown || Target.lifeCycle.isFaulted) return;
			Target.gameObject.SetActive(false);
			Target.transform.SetParent(roots[Key], false);
			pools[Key].Release(Target.gameObject);
		}

		public override void Uninitialize() {
			ShuttingDown = true;
			foreach (UUI Target in Owned) if (Target) Target.lifeCycle.Shutdown();
			foreach (UUIQueue Queue in uuiQueue.Values) Queue.Cancel();
			uuiQueue.Clear();
			base.Uninitialize();
		}

		public void AddUUIQueue(string key, UUIQueueItem queueItem) {
			uuiQueue.TryAdd(key, new UUIQueue());
			uuiQueue[key].queue.Enqueue(queueItem);
			uuiQueue[key].Next(key);
		}
	}
}
