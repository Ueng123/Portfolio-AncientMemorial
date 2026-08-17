using System;
using System.Collections.Generic;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.Utility {
	public static class CacheManager {
		private static Dictionary<int, WaitForSeconds> waitForSecondsCache = new ();
		
		public static WaitForSeconds WaitForSeconds(float duration) {
			int secondT = Mathf.RoundToInt(duration * 1000);
			return GetObject(secondT);
		}
		
		public static WaitForSeconds WaitForSecondsFlooring(float duration) {
			int secondT = Mathf.FloorToInt(duration * 1000);
			return GetObject(secondT);
		}

		public static WaitForSeconds WaitForSecondsCeiling(float duration) {
			int secondT = Mathf.CeilToInt(duration * 1000);
			return GetObject(secondT);
		}

		private static WaitForSeconds GetObject(int secondT) {
			DebugManager.Log($"try to get WaitForSeconds({secondT/1000f}) from cache");
			if (secondT == 0) return null;
			
			if (waitForSecondsCache.TryGetValue(secondT, out WaitForSeconds waitForSeconds)) {
				DebugManager.Log($"got WaitForSeconds({secondT/1000f}) from cache");
				return waitForSeconds;
			}
			
			DebugManager.Log($"not found. get new object of WaitForSeconds({secondT/1000f})");
			waitForSeconds = new WaitForSeconds(secondT/1000f);
			waitForSecondsCache.Add(secondT, waitForSeconds);
			return waitForSeconds;
		}
	}
}