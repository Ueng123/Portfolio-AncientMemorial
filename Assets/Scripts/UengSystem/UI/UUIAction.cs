using System;
using UengSystem.Logic.Tasks;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIAction {
		public          UUIComponent component;
		public abstract void         Initialize(ITaskable self);
		public abstract void         Routine(ITaskable self);
	}
}