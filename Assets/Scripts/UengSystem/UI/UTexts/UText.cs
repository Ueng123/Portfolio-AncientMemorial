using System;
using TMPro;
using UnityEngine;

namespace UengSystem.UI.UTexts {
	[Serializable]
	public class UText : UUIComponent {
		[HideInInspector]
		public TMP_Text text;

		public override void Initialize() {
			text = GetComponent<TMP_Text>();
		}
		
		public string GetValue()             => text.text;
		public void   SetValue(string value) => text.text = value;
	}
}