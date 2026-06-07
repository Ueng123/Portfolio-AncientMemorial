using System;
using UengSystem.Logic.Tasks;
using UengSystem.Objects;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIAction {
		public          UUIComponent component;
		public abstract void         Initialize(UObject self);
		public abstract void         Routine(UObject self);
	}
}