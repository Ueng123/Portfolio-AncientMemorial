using System;
using UengSystem.Tasks;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UIAction {
		public abstract void Routine(ITaskable self);
	}
}