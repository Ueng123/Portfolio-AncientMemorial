using System.Collections.Generic;
using System.Linq;
using AncientMemorial;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI.UUIQueues {
	public class UUIQueue {
		public Queue<UUIQueueItem> queue = new Queue<UUIQueueItem>();

		private bool canDoNext = true;
		public void GetNextQueue(string key) {
			if (queue.Count == 0) return;
			if (!canDoNext) return;
			
			UUIQueueItem item = queue.Dequeue();
			item.data.PreGet();
			
			GameObject    ui   = UUIObjectPool.instance.Open(key, GameManager.instance.mainScreenCanvas);
			UUI           uui  = ui.GetComponent<UUI>();
			UUIPrefabItem data = UUIObjectPool.instance.prefabData.Where(data => data.prefab.name == key).ToArray()[0];
			item.data.Initialize(uui);
			
			uui.ID = key;
			
			canDoNext = false;
			new DelayedAction(data.openTime + 0.05f, ()=> {
				new WaitAction(
					() => !UObject.UObjectExists(key),
					() => new DelayedAction(
						data.closeTime + item.marginFront + 0.01f,
						() => { canDoNext = true; }).Execute(),
					() => {
						if (uui.isReleased) return;
						UUIObjectPool.instance.Close(ui);
						new DelayedAction(data.closeTime + item.marginBack + 0.01f,
										  () => { canDoNext = true; }).Execute();
					}, item.duration).Execute();
			}).Execute();
		}
	}
}