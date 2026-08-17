using System.Collections;
using System.Collections.Generic;
using AncientMemorial;
using UengSystem.Managers;
using UengSystem.UDebug;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI.UUIQueues {
	public class UUIQueue {
		public  Queue<UUIQueueItem> queue        = new Queue<UUIQueueItem>();
		private bool                nextAvailable = true;
		
		public void Next(string key) {
			if (!nextAvailable) return;
			nextAvailable = false;
			
			UUIQueueItem item = queue.Dequeue();
			
			CoroutineRunner.instance.StartCoroutine(Routine(key, item));
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
			WaitForSeconds marginFront = CacheManager.WaitForSecondsCeiling (queueItem.marginFront);
			WaitForSeconds marginBack  = CacheManager.WaitForSecondsFlooring(queueItem.marginBack);
			
			yield return marginFront;
			
			GameObject itemObject = UUIPool.instance.Open(key, GameManager.instance.mainScreenCanvas);
			UUI        itemUI     = itemObject.GetComponent<UUI>();
			itemUI.ID = key;
			
			queueItem.data.Initialize(itemUI);
			
			WaitForSeconds openTime  = CacheManager.WaitForSecondsCeiling (itemUI.openTime);
			WaitForSeconds closeTime = CacheManager.WaitForSecondsFlooring(itemUI.closeTime);
			
			yield return openTime;

			float elapsed = 0f;
			while (!itemUI.isReleased && elapsed < queueItem.duration) {
				elapsed += Time.deltaTime;
				yield return null;
			}
			
			if (!itemUI.isReleased) UUIPool.instance.Close(itemUI.gameObject);
			yield return closeTime;
			yield return marginBack;

			nextAvailable = true;
			if (queue.Count != 0) Next(key);
		}
	}
}