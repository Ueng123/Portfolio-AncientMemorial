using System;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.UI {
	[Serializable]
	public abstract class UUIComponent : UObject {

		// 인스턴스 프로퍼티
		[Header("UI Component")]
		public UUI parent;
	}
}