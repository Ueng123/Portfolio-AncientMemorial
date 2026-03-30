using System;
using AncientMemorial.Tasks;

namespace AncientMemorial.Waves {
	[Serializable]
	public abstract class WaveCondition {
		public abstract bool Check();

		public TaskList taskToDo;
	}
}