using System.Collections;
using System.Collections.Generic;
using AncientMemorial;
using UengSystem.Managers;
using UengSystem.UDebug;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI.UUIQueues {
	public class UUIQueue {

		// 인스턴스 프로퍼티
		private readonly Dictionary<string, int> PrefabIds = new();

		public  Queue<UUIQueueItem> queue        = new Queue<UUIQueueItem>();
		private bool                nextAvailable = true;
		private Coroutine Running;
		private GlobalObject Runner;
		private bool Cancelled;

		// 인스턴스 메서드
		private int GetPrefabId(string Key) {
			if (!PrefabIds.TryGetValue(Key, out int PrefabId)) {
				PrefabId = Key.GetHash();
				PrefabIds.Add(Key, PrefabId);
			}
			return PrefabId;
		}

		public void Cancel() {
			Cancelled = true;
			queue.Clear();
			if (Runner && Running != null) Runner.StopCoroutine(Running);
			Running = null;
		}
		
		public void Next(string key) {
			if (Cancelled || !nextAvailable || queue.Count == 0) return;
			nextAvailable = false;
			
			UUIQueueItem item = queue.Dequeue();
			
			Runner = GlobalObject.instance;
			Running = Runner.StartCoroutine(Routine(key, item));
			// new DelayedAction(itemUI.openTime + 0.05f, ()=> {
			// 	new WaitAction(
			// 		() => !UObject.UObjectExists(key),
			// 		() => new DelayedAction(
			// 			itemUI.closeTime + item.marginFront + 0.01f,
			// 			() => { canDoNext = true; }).ExecuteDA(),
			// 		() => {
			// 			if (!itemUI.isReleased) UUIObjectPool.instance.Close(itemObject);
			// 			new DelayedAction(itemUI.closeTime + item.marginBack + 0.01f,
			// 							  () => { canDoNext = true; }).ExecuteDA();
			// 		}, item.duration).ExecuteWA();
			// }).ExecuteDA();
		}

		public IEnumerator Routine(string key, UUIQueueItem queueItem) {
			WaitForSecondsRealtime marginFront = new(queueItem.marginFront);
			WaitForSecondsRealtime marginBack = new(queueItem.marginBack);
			
			yield return marginFront;
			
			if (Cancelled) yield break;
			GameObject itemObject = UUI.Get(GetPrefabId(key), GameManager.instance.mainScreenCanvas, Configure: Ui => {
				Ui.ID = key;
				queueItem.data.Initialize(Ui);
			});
			UUI        itemUI     = itemObject.GetComponent<UUI>();
			long Life = itemUI.lifeNumber;
			while (itemUI && itemUI.Matches(Life) && itemUI.lifeCycle.phase == Objects.LifeCycle.LifeCyclePhase.Getting) yield return null;

			float elapsed = 0f;
			while (itemUI && itemUI.Matches(Life) && itemUI.isActive && elapsed < queueItem.duration) {
				elapsed += Time.unscaledDeltaTime;
				yield return null;
			}
			
			if (itemUI && itemUI.Matches(Life) && !itemUI.isReleased) itemUI.Release();
			while (itemUI && itemUI.Matches(Life) && !itemUI.isReleased) yield return null;
			yield return marginBack;

			nextAvailable = true;
			if (queue.Count != 0) Next(key);
		}
	}
}
