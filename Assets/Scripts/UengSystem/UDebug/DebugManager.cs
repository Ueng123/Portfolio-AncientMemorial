using System;
using System.Diagnostics;
using UengSystem.Managers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UengSystem.UDebug {
	public class DebugManager : Manager<DebugManager> {

		// 인스턴스 프로퍼티
		public bool debugMode = true;
		public bool Logging   = true;

		// 정적 메서드
		[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string message) {
			if (instance && instance.Logging) return;
			Debug.Log($"<color=#eef>[LOG]\n{message}\n\n</color>");
		}
		
		[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
		public static void LogWarning(string message) {
			if (instance && instance.Logging) return;
			Debug.LogWarning($"<color=#fda>[WARN]\n{message}\n\n</color>");
		}
		
		public static void LogException(Exception Error, UnityEngine.Object Context) {
			Debug.LogException(Error, Context);
		}

		public static void LogError(string message, Exception _e, GameObject obj) {
			Debug.LogError($"<color=#fda><size=15><b>{message}</b></size></color>");
			Debug.LogException(_e, obj);
		}
	}
}
