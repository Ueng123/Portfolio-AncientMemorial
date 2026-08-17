using System;
using TMPro;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UText : UUIComponent {
		[HideInInspector]
		public TMP_Text text;
		private string textCache = null;

		public override void Initialize() {
			text      = GetComponent<TMP_Text>();
			textCache = text.text;
		}
		
		public string GetValue()             => textCache;
		public void   SetValue(string value) {
			if (textCache!=null && value == textCache) return;
			textCache = value;
			text.text = value;
		}
	}
}