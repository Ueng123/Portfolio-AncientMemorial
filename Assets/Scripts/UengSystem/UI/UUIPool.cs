using System.Collections.Generic;
using UengSystem.Managers;
using System;
using UengSystem.Objects;
using UengSystem.ObjectPool;
using UengSystem.UDebug;
using UengSystem.UI.UUIQueues;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace UengSystem.UI {
	public class UUIPool : Manager<UUIPool> {

		// 인스턴스 프로퍼티
		public GameObject[] prefabList;
		private readonly Dictionary<int, ObjectPool<GameObject>> pools            = new (15);
		private readonly Dictionary<int, Transform>              roots            = new (15);
		private readonly Dictionary<string, UUIQueue>               uuiQueue         = new (5);
		private readonly HashSet<UUI> Owned = new();
		private bool ShuttingDown;

		// 인스턴스 메서드
		public UUI Acquire(string PrefabKey, UCanvas Canvas) {
			if (PrefabKey == null) throw new ArgumentNullException(nameof(PrefabKey));
			return Acquire(PrefabKey.GetHash(), Canvas);
		}

		public UUI Acquire(int PrefabId, UCanvas Canvas) {
			if (ShuttingDown) throw new InvalidOperationException("UUIPool is shutting down.");
			if (!Canvas) throw new ArgumentNullException(nameof(Canvas));
			if (!pools.TryGetValue(PrefabId, out ObjectPool<GameObject> Pool))
				throw new KeyNotFoundException("Unregistered UI prefab ID: " + PrefabId);
			
			UUI Target = Pool.Get().GetComponent<UUI>();
			Target.transform.SetParent(Canvas.transform, false);
			Target.canvas = Canvas;
			return Target;
		}
		
		private void ReturnToPool(int Key, UUI Target) {
			if (ShuttingDown || Target.lifeCycle.isShuttingDown || Target.lifeCycle.isFaulted) return;
			Target.gameObject.SetActive(false);
			Target.transform.SetParent(roots[Key], false);
			pools[Key].Release(Target.gameObject);
		}

		public void AddUUIQueue(string key, UUIQueueItem queueItem) {
			uuiQueue.TryAdd(key, new UUIQueue());
			uuiQueue[key].queue.Enqueue(queueItem);
			uuiQueue[key].Next(key);
		}

		// 오버라이드 메서드
		public override void Initialize() {
			if (pools.Count != 0) throw new InvalidOperationException("UUIPool is already initialized.");
			var Keys = new Dictionary<int, string>();
			foreach (GameObject Prefab in prefabList) {
				if (!Prefab || !Prefab.GetComponent<UUI>())
					throw new InvalidOperationException("UI prefabs must contain UUI components.");
				int PrefabId = Prefab.name.GetHash();
				if (Keys.TryGetValue(PrefabId, out string ExistingName))
					throw new InvalidOperationException($"Duplicate UI prefab ID {PrefabId}: '{ExistingName}' and '{Prefab.name}'.");
				Keys.Add(PrefabId, Prefab.name);
			}
			pools.Clear();
			roots.Clear();
			uuiQueue.Clear();
			
			foreach (GameObject prefab in prefabList) {
				int PrefabId = prefab.name.GetHash();
				Transform newRoot  = new GameObject($"{prefab.name} root").transform;
				roots[PrefabId] = newRoot;
				newRoot.parent     = transform;
				newRoot.gameObject.SetActive(false);
				
				pools[PrefabId] = new ObjectPool<GameObject>(
					createFunc: () => {
						GameObject obj = Instantiate(prefab, newRoot);
						obj.name = prefab.name;
							
						obj.SetActive(false);
						UUI Target = obj.GetComponent<UUI>();
						Target.OnFirstGet();
						Target.SetPoolReturnMethod(Item => ReturnToPool(PrefabId, (UUI)Item));
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

		public override void Uninitialize() {
			ShuttingDown = true;
			foreach (UUI Target in Owned) if (Target) Target.lifeCycle.Shutdown();
			foreach (UUIQueue Queue in uuiQueue.Values) Queue.Cancel();
			uuiQueue.Clear();
			base.Uninitialize();
		}
	}
}
