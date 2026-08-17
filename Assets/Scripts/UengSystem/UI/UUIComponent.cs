using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIComponent : UObject {
		[Header("UI Component")]
		public UUI parent;
	}
}