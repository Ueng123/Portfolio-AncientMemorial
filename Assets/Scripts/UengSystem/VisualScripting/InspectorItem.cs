using System;
using UnityEngine;

namespace UengSystem.VisualScripting {
	[Serializable]
	public abstract class InspectorItem : ISerializationCallbackReceiver {
		public virtual void OnBeforeSerialize() { }

		public virtual void OnAfterDeserialize() { }
	}
}