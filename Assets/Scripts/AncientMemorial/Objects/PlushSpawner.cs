using System.Collections.Generic;
using UengSystem.Objects;
using AncientMemorial.Interactions;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class PlushSpawner : Crystal {

		// 인스턴스 프로퍼티
		private readonly Dictionary<GameObject, int> PrefabIds = new();

		StopWatch spawnTimer;

		public GameObject[] plushList;

		// 인스턴스 메서드
		private int GetPrefabId(GameObject Prefab) {
			if (!PrefabIds.TryGetValue(Prefab, out int PrefabId)) {
				PrefabId = Prefab.name.GetHash();
				PrefabIds.Add(Prefab, PrefabId);
			}
			return PrefabId;
		}

		// 오버라이드 메서드
		public override void Initialize() {
			base.Initialize();

			spawnTimer = new StopWatch();
			spawnTimer.Tick();
		}
		
		protected override void Routine() {
			bool downCondition = InputManager.GetInput(ActionType.MouseLClick, InputState.Down);
			bool holdCondition = InputManager.GetInput(ActionType.MouseLClick, InputState.Hold) && spawnTimer.CheckOut(0.1f);

			if (!downCondition && !holdCondition) return;
			
			spawnTimer.Tick();
			int plushName = GetPrefabId(plushList[Random.Range(0, plushList.Length)]);
			Vector2 pos       = InputManager.mousePosition;
			pos = new Vector2(Mathf.Clamp(pos.x, -5, 5), Mathf.Clamp(pos.y, 1, 5));
			
			UObject.Get(plushName, pos, PlayEffect: true);
		}
	}
}
