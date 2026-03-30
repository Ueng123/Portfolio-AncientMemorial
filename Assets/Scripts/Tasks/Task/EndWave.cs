using System;
using AncientMemorial.Waves;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class EndWave : Task {
		public override void Execute(ITaskable self) {
			WaveManager.instance.EndWave();
		}
	}
}