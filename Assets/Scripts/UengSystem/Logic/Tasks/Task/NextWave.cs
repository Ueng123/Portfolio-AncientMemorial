using System;
using AncientMemorial.Waves;
using UengSystem.Logic.Tasks;
using UengSystem.UDebug;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class NextWave : TaskComponent {
		public override void Execute(ITaskable self) {
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			WaveManager.instance.NextWave();
		}
	}
}