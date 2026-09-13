using System;
using UengSystem.Objects;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIAction {

		// 인스턴스 프로퍼티
		public          UUIComponent component;

		// 인스턴스 메서드
		public abstract void         Initialize(UObject   self);
		public abstract void         Uninitialize(UObject self);
		public abstract void         Routine(UObject      self);

		public virtual T GetComponent<T>() {
			return (T)(object)component;
		}
	}
}