using System;
using TMPro;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UText : UUIComponent {

		// 인스턴스 프로퍼티
		[HideInInspector]
		public TMP_Text text;
		private string textCache = null;

		// 인스턴스 메서드
		public string GetValue()             => textCache;
		public void   SetValue(string value) {
			if (textCache!=null && value == textCache) return;
			textCache = value;
			text.text = value;
		}

		// 오버라이드 메서드
		public override void Initialize() {
			text      = GetComponent<TMP_Text>();
			textCache = text.text;
		}
	}
}