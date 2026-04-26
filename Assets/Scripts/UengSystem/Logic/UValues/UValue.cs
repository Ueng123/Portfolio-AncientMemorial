using System;
using UnityEngine;

namespace UengSystem.Logic.UValues {
	[Serializable]
	public abstract class UValue<T> : IUValue {
		public IUValue parent;
		
		public abstract bool  getIsDynamic { get; }
		public          bool? isDynamicCache;
		public          bool  isDynamicAuto {
			get {
				isDynamicCache ??= getIsDynamic;
				return (bool)isDynamicCache;
			}
		}

		[Header("UValue Property")]
		public DynamicType dynamicType;

		public bool isDynamic => dynamicType == DynamicType.Auto?isDynamicAuto:dynamicType == DynamicType.Dynamic;
		
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