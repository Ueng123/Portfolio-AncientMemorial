using System;
using UengSystem.Tasks;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIAction {
		public          UUIComponent component;
		public abstract void         Routine(ITaskable self);
	}
}