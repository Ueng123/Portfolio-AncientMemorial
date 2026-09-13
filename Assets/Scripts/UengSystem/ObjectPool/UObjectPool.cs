using System.Collections.Generic;
using UengSystem.Managers;
using System;
using UengSystem.Objects;
using UengSystem.Utility;

using UnityEngine;
using UnityEngine.Pool;

namespace UengSystem.ObjectPool {
	public class UObjectPool : Manager<UObjectPool> {

		// 인스턴스 프로퍼티
		[SerializeField]
		private GameObject[]                               prefabs;

		private          Dictionary<int, ObjectPool<GameObject>> pools;
		private          Dictionary<int, Transform>              roots;
		private readonly HashSet<UObject>                        Owned = new();
		private          bool                                    ShuttingDown;

		// 인스턴스 메서드
		public bool Contains(string PrefabKey) => PrefabKey != null && Contains(PrefabKey.GetHash());
		public bool Contains(int PrefabId) => pools != null && pools.ContainsKey(PrefabId);

		public UObject Acquire(string PrefabKey, Vector2 Position) {
			if (PrefabKey == null) throw new ArgumentNullException(nameof(PrefabKey));
			return Acquire(PrefabKey.GetHash(), Position);
		}

		public UObject Acquire(int PrefabId, Vector2 Position) {
			if (ShuttingDown) throw new InvalidOperationException("UObjectPool is shutting down.");
			if (pools == null || !pools.TryGetValue(PrefabId, out ObjectPool<GameObject> Pool))
				throw new KeyNotFoundException("Unregistered UObject prefab ID: " + PrefabId);
			
			GameObject Obj = Pool.Get();
			Obj.transform.SetParent(transform);
			Obj.transform.position = Position;
			return Obj.GetComponent<UObject>();
		}

		private void ReturnToPool(int PrefabId, UObject Target) {
			if (ShuttingDown || Target.lifeCycle.isShuttingDown || Target.lifeCycle.isFaulted) return;
			Target.gameObject.SetActive(false);
			Target.transform.SetParent(roots[PrefabId]);
			pools[PrefabId].Release(Target.gameObject);
		}

		// 오버라이드 메서드
		public override void Initialize() {
			if (pools != null) throw new InvalidOperationException("UObjectPool is already initialized.");
			Dictionary<int, string> Keys = new ();
			foreach (GameObject Prefab in prefabs) {
				if (!Prefab || !Prefab.GetComponent<UObject>() || Prefab.GetComponent<UI.UUI>())
					throw new InvalidOperationException("UObject prefabs must be non-UI UObject prefabs.");
				int PrefabId = Prefab.name.GetHash();
				if (Keys.TryGetValue(PrefabId, out string ExistingName))
					throw new InvalidOperationException($"Duplicate UObject prefab ID {PrefabId}: '{ExistingName}' and '{Prefab.name}'.");
				Keys.Add(PrefabId, Prefab.name);
			}
			
			pools = new Dictionary<int, ObjectPool<GameObject>>();
			roots = new Dictionary<int, Transform>();
			
			foreach (GameObject prefab in prefabs) {
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
							UObject Target = obj.GetComponent<UObject>();
							Target.OnFirstGet();
							Target.BindPool(Item => ReturnToPool(PrefabId, Item));
							Owned.Add(Target);
							return obj;
						},
						actionOnGet: (obj) => {  },
						actionOnRelease: (obj) => {  },
						actionOnDestroy: Obj => { Owned.Remove(Obj.GetComponent<UObject>()); Destroy(Obj); },
						collectionCheck: true,
						defaultCapacity: 10,
						maxSize: 50
				); 
			}
		}

		public override void Uninitialize() {
			ShuttingDown = true;
			foreach (UObject Target in Owned) if (Target) Target.lifeCycle.Shutdown();
			base.Uninitialize();
		}
	}
}
