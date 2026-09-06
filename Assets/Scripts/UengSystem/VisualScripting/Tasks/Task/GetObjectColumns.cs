using System;
using System.Collections;
using AncientMemorial.Map;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UVector2s;
// using UengSystem.VisualScripting.UValues.UVector2s;
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

		private WaitForSeconds waitSecondCache;

		public GetObject ConfigureTask(GetObject task, Vector2 position) {
			task.position = new UPureVector2 {pureValue = position};
			return task;
		}
		
		public override void Execute(ITaskable self) {
			float mapSizeX                                   = MapManager.instance.GetTargetMapSize().x;
			if (!(term?.isDynamic ?? false)) waitSecondCache = new WaitForSeconds(term?.value??0);
			
			StartCoroutine(GetObjects(mapSizeX/2f, self));
		}

		public IEnumerator GetObjects(float mapSizeXHalf, ITaskable self) {
			for (float x = 0; x < mapSizeXHalf; x+=distance) {
				if (x == 0) {
					ConfigureTask(getTask, new Vector2(x, constY?.value??0));
					getTask.Execute(self);
				}
				else {
					ConfigureTask(getTask, new Vector2(x, constY?.value??0));
					getTask.Execute(self);
					
					ConfigureTask(getTask, new Vector2(-x, constY?.value??0));
					getTask.Execute(self);
				}

				yield return self is UI.UUI ? new WaitForSecondsRealtime(term?.value ?? 0) : waitSecondCache ?? new WaitForSeconds(term.value);
			}
		}
	}
}
