using System;
using AncientMemorial.Waves;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class NextWave : Task {
		public override void Execute(ITaskable self) {
			WaveManager.instance.NextWave();
		}
	}
}