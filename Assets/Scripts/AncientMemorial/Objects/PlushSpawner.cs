using AncientMemorial.Interactions;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Objects;
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

		protected override void EarlyRoutine() { }
		
		protected override void Routine() {
			bool downCondition = InputManager.inputData[InputActionType.MouseLClick].pressType == InputPressType.Down
								 && !spawnTimer.Check(0.1f);
			bool holdCondition = InputManager.inputData[InputActionType.MouseLClick].pressType == InputPressType.Hold
								 && !spawnTimer.Check(0.2f);

			if (!downCondition && !holdCondition) return;
			
			spawnTimer.Tick();
			string  plushName = plushList[Random.Range(0, plushList.Length)].name;
			Vector2 pos       = InputManager.inputData[InputActionType.MousePosition].valueV;
			pos = new Vector2(Mathf.Clamp(pos.x, -5, 5), Mathf.Clamp(pos.y, 1, 5));
			
			UObjectPool.instance.Get(plushName, pos, 3);
		}

		protected override void LateRoutine() { }

		protected override void FixedRoutine() { }
	}
}