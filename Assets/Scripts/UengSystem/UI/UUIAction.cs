using System;
using UengSystem.Objects;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIAction {
		public          UUIComponent component;
		public abstract void         Initialize(UObject   self);
		public abstract void         Uninitialize(UObject self);
		public abstract void         Routine(UObject      self);

		public virtual T GetComponent<T>() {
			return (T)(object)component;
		}
	}
}