using System;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues {
	[Serializable]
	public abstract class UValue<T> : InspectorItem, IUValue {

		// 인스턴스 프로퍼티
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

		// 인스턴스 메서드
		public virtual UValue<T> OnSet() {
			return this;
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

		// 오버라이드 메서드
		public override void OnAfterDeserialize() {
			isValueCached  = false;
			isDynamicCache = null;
		}
	}
}