using System;
using AncientMemorial.Map;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ChangeMapSize : TaskComponent {

		// 인스턴스 프로퍼티
		public Vector2 newSize;
		public float   newSpeed;
		public bool    instant;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			string speed = instant ? "Instant" : newSize.ToString();
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data"              +
							 $" > target size = {newSize}\n" +
							 $" > speed = {speed}");
			
			MapManager.instance.SetMapSize(newSize, newSpeed, instant);
		}
	}
}