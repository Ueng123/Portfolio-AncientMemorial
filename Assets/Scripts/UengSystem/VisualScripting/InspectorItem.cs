using System;
using UnityEngine;

namespace UengSystem.VisualScripting {
	[Serializable]
	public abstract class InspectorItem : ISerializationCallbackReceiver {

		// 인스턴스 메서드
		public virtual void OnBeforeSerialize() { }

		public virtual void OnAfterDeserialize() { }
	}
}