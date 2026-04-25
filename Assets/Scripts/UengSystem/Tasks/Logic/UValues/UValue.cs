using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UengSystem.Tasks.Logic.UValues {
	[Serializable]
	public abstract class UValue<T> : IUValue {
		public IUValue parent;
		
		public abstract bool  getIsDynamic { get; }
		public          bool? isDynamicCache;
		public          bool  isDynamic {
			get {
				isDynamicCache ??= getIsDynamic;
				return (bool)isDynamicCache;
			}
		}
		
		public abstract          T    getValue     { get; }
		[HideInInspector] public T    valueCache;
		[HideInInspector] public bool isValueCached = false;
		public          T value {
			get {
				if (isDynamic) return getValue;
				if (isValueCached) return valueCache;
				
				valueCache = getValue;
				isValueCached = true;
				return valueCache;
			}
		}
		
		public void ResetCache() {
			isValueCached  = true;
			isDynamicCache = null;
			parent?.ResetCache();
		}
	}
}