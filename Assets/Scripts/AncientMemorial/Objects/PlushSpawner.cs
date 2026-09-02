using AncientMemorial.Interactions;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class PlushSpawner : Crystal {
		StopWatch spawnTimer;

		public GameObject[] plushList;
		
		public override void Initialize() {
			base.Initialize();

			spawnTimer = new StopWatch();
			spawnTimer.Tick();
		}
		
		protected override void Routine() {
			bool downCondition = InputManager.GetInput(ActionType.MouseLClick, PressType.Down);
			bool holdCondition = InputManager.GetInput(ActionType.MouseLClick, PressType.Hold) && spawnTimer.CheckOut(0.1f);

			if (!downCondition && !holdCondition) return;
			
			spawnTimer.Tick();
			string  plushName = plushList[Random.Range(0, plushList.Length)].name;
			Vector2 pos       = InputManager.mousePosition;
			pos = new Vector2(Mathf.Clamp(pos.x, -5, 5), Mathf.Clamp(pos.y, 1, 5));
			
			UObjectPool.instance.Get(plushName, pos, 3);
		}
	}
}
