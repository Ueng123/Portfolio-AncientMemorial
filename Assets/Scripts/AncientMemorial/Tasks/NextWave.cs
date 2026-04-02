using System;
using AncientMemorial.Waves;
using UengSystem.Tasks;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class NextWave : TaskComponent {
		public override void Execute(ITaskable self) {
			WaveManager.instance.NextWave();
		}
	}
}