using System;

namespace UengSystem.Tasks.Logic.UValues {
	[Serializable]
	public abstract class UValue<T> : IUValue {
		public IUValue parent;
		
		public abstract bool getIsDynamic { get; }
		public          bool? isDynamicCache;
		public          bool  isDynamic {
			get {
				isDynamicCache ??= getIsDynamic;
				return (bool)isDynamicCache;
			}
		}

		public abstract T getValue     { get; }
		public          T valueCache;
		public T value {
			get {
				if (isDynamic) return getValue;
				valueCache ??= getValue;
				return valueCache;
			}
		}
		
		public void ResetCache() {
			isDynamicCache = null;
			valueCache     = default;
			parent?.ResetCache();
		}
	}
}