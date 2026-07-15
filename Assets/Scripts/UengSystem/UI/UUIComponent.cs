using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIComponent : BasicUObject {
		[Header("UI Component")]
		public UUI parent;
	}
}