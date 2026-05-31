using System;
using UnityEngine;

namespace UengSystem.Logic.UValues {
	[Serializable]
	public abstract class UValue<T> : IUValue, ISerializationCallbackReceiver { // ISerializ... : 런타임에서 캐싱이 되면 그게 초기화 안됨.
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
			isValueCached  = false;
			isDynamicCache = null;
			parent?.ResetCache();
		}

		public void OnBeforeSerialize() { }

		public void OnAfterDeserialize() {
			isValueCached  = false;
			isDynamicCache = null;
		}
	}
}