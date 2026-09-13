using System;
using AncientMemorial.Waves;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class NextWave : TaskComponent {

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			WaveManager.instance.NextWave();
		}
	}
}