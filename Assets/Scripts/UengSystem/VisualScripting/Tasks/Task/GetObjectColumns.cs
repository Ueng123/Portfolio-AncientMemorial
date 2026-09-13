using System;
using System.Collections;
using AncientMemorial.Map;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UVector2s;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class GetObjectColumns : TaskComponent {
		public float distance;
		public GetObject getTask;

		[SerializeReference] [SubclassSelector]
		public UValue<float> term;
		[SerializeReference] [SubclassSelector]
		public UValue<float> constY;

		public GetObject ConfigureTask(GetObject task, Vector2 position) {
			task.position = new UPureVector2 { pureValue = position };
			return task;
		}

		public IEnumerator GetObjects(float mapSizeXHalf, ITaskable self) {
			bool isDynamicTerm = term?.isDynamic ?? false;
			float fixedTerm = isDynamicTerm ? 0 : term?.value ?? 0;
			WaitForSeconds fixedWait = isDynamicTerm ? null : CacheManager.WaitForSecondsCeiling(fixedTerm);
			for (float x = 0; x < mapSizeXHalf; x += distance) {
				ConfigureTask(getTask, new Vector2(x, constY?.value ?? 0));
				getTask.Execute(self);

				if (x != 0) {
					ConfigureTask(getTask, new Vector2(-x, constY?.value ?? 0));
					getTask.Execute(self);
				}
				
				yield return isDynamicTerm ? CacheManager.WaitForSecondsCeiling(term?.value ?? 0) : fixedWait;
			}
		}

		public override void Execute(ITaskable self) {
			float mapSizeX = MapManager.instance.GetTargetMapSize().x;
			StartCoroutine(GetObjects(mapSizeX / 2f, self));
		}
	}
}
