using System;

namespace AncientMemorial.Waves {
	[Serializable]
	public abstract class WaveEndCondition {
		public abstract bool Check();
	}
}