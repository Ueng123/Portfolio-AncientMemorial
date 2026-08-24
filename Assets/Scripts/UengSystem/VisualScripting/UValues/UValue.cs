using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues {
	[Serializable]
	public abstract class UValue<T> : ISerializationCallbackReceiver, IUValue { // ISerializ... : 런타임에서 캐싱이 되면 그게 초기화 안됨.
		public IUValue parent;

		protected abstract bool  getIsDynamic { get; }
		private            bool? isDynamicCache = null;
		private            bool  isDynamicAuto {
			get {
				isDynamicCache ??= getIsDynamic;
				return (bool)isDynamicCache;
			}
		}

		[Header("UValue Property")]
		public DynamicType dynamicType;
		public bool isDynamic => dynamicType == DynamicType.Auto?isDynamicAuto:dynamicType == DynamicType.Dynamic;

		public virtual UValue<T> OnSet() {
			return this;
		}
		
		// DYNAMIC TYPE
		// 1. AUTO 이면
		//  초회 getIsDynamic 호출 및 캐싱
		//  이후 값 가져올때 캐싱된 값 제공.
		// 2. AUTO가 아니면
		//  DynamicType 값에 따라 판별
		
		// VALUE
		// 1. 동적 값일때
		//  온디맨드 방식을 통한 값의 유효성 보장
		// 2. 정적 값일때
		//  초회 getValue 호출 및 캐싱
		//  이후 값 가져올때 캐싱된 값 제공
		
		protected abstract        T    getValue     { get; }
		private T    valueCache;
		private bool isValueCached = false;
		public          T value {
			get {
				if (isDynamic) return getValue;
				if (isValueCached) return valueCache;
				
				valueCache = getValue;
				isValueCached = true;
				return valueCache;
			}
		}

		public UValue<T> SetDynamicCache(DynamicType dynamicType) {
			this.dynamicType = dynamicType;
			return this;
		}
		
		public virtual void Dirty() {
			isValueCached  = false;
			isDynamicCache = null;
			parent?.Dirty();
		}
		
		public virtual void OnBeforeSerialize() { }

		public virtual void OnAfterDeserialize() {
			isValueCached  = false;
			isDynamicCache = null;
		}
	}
}